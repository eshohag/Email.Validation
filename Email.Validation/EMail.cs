using DnsClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace Email.Validation
{
    public abstract class EMail
    {
        public static Response IsValidEmail(string email)
        {
            var isSyntaxValidEmail = ValidateEmail(email, ValidationModeEnum.Syntax);
            var mailServers = GetNetworkValidation(isSyntaxValidEmail, email);

            var isNetworkValidEmail = ValidateEmail(email, ValidationModeEnum.Network, isSyntaxValidEmail, mailServers);
            var isHandshakingValidEmail = ValidateEmail(email, ValidationModeEnum.HandShaking, isSyntaxValidEmail, mailServers);

            var result = new Response()
            {
                SuccessStatus = true,
                SyntaxValidationStatus = isSyntaxValidEmail,
                MXValidationStatus = isNetworkValidEmail,
                HandshakingValidationStatus = isHandshakingValidEmail,

                Domain = isSyntaxValidEmail ? email.Split('@')[1] : string.Empty,
                Email = isSyntaxValidEmail ? email : string.Empty
            };
            if (!isSyntaxValidEmail || !isNetworkValidEmail || !isHandshakingValidEmail)
                result.SuccessStatus = false;

            return result;
        }

        private static bool ValidateEmail(string emailAddress, ValidationModeEnum validationMode, bool isSyntexValid = false, List<MailServer> mailServers = null)
        {
            string getLastError = String.Empty;

            switch (validationMode)
            {
                case ValidationModeEnum.Syntax:
                    try
                    {
                        string pattern = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" + @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" + @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
                        Regex regex = new Regex(pattern);
                        return regex.IsMatch(emailAddress);
                    }
                    catch (Exception ex)
                    {
                        getLastError = string.Format("Could not match pattern because of the error {0} ", ex.Message);
                    }
                    return false;

                case ValidationModeEnum.Network:
                    try
                    {
                        if (!isSyntexValid)
                            return false;
                        if (mailServers.Count > 0)
                            return true;
                    }
                    catch (Exception ex)
                    {
                        getLastError = string.Format("Could not match pattern because of the error {0} ", ex.Message);
                    }
                    return false;

                case ValidationModeEnum.HandShaking:
                    if (!isSyntexValid)
                        return false;
                    if (mailServers.Count == 0)
                        return false;

                    foreach (var aServer in mailServers)
                    {
                        var smtpClient = new SmtpClient(aServer.ServerName);
                        smtpClient.EnableSsl = true;
                        TcpClient tcpClient = new TcpClient();
                        try
                        {
                            tcpClient.Connect(smtpClient.Host, smtpClient.Port);
                            if (tcpClient.Connected)
                            {
                                string serverResponse = String.Empty;
                                NetworkStream netStream = tcpClient.GetStream();
                                StreamWriter streamWriter = new StreamWriter(netStream);
                                StreamReader streamReader = new StreamReader(netStream);

                                serverResponse = streamReader.ReadLine();

                                if (serverResponse != null && serverResponse.Contains("220"))   //Connected SMTP Server
                                {
                                    /* Perform HELO to SMTP Server and get Response */
                                    streamWriter.WriteLine("HELO ESMTP");
                                    streamWriter.Flush();
                                    serverResponse = streamReader.ReadLine();

                                    streamWriter.WriteLine("mail from:<shohagdsonline@gmail.com>");
                                    streamWriter.Flush();
                                    serverResponse = streamReader.ReadLine();

                                    /* Read Response of the RCPT TO Message to know from google if it exist or not */
                                    streamWriter.WriteLine($"rcpt to:<{emailAddress}>");
                                    streamWriter.Flush();
                                    serverResponse = streamReader.ReadLine();

                                    if (serverResponse != null && serverResponse.Contains("250"))
                                    {
                                        tcpClient.Close();
                                        return true;
                                    }
                                }
                                tcpClient.Close();
                            }
                        }
                        catch (Exception ex)
                        {
                            var messsage = ex.Message;
                            tcpClient.Close();
                        }
                    }
                    return false;

                default:
                    return false;
            }
        }

        private static List<MailServer> GetNetworkValidation(bool isSyntaxValidEmail, string emailAddress)
        {
            if (!isSyntaxValidEmail)
                return null;

            var servers = new List<MailServer>();
            var client = new LookupClient();

            string[] arrHost = emailAddress.Split('@');
            var result = client.Query(arrHost[1], QueryType.MX);

            foreach (var item in result.Answers)
            {
                var X = item as DnsClient.Protocol.MxRecord;
                servers.Add(new MailServer() { ServerName = X.Exchange, Preference = X.Preference });
            }
            return servers.OrderBy(a => a.Preference).ToList();
        }
    }
}
