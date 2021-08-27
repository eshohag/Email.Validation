namespace Email.Validation
{
    public class Response
    {
        public bool SuccessStatus { get; set; }
        public bool SyntaxValidationStatus { get; set; }
        public bool MXValidationStatus { get; set; }
        public bool HandshakingValidationStatus { get; set; }

        public string Email { get; set; }
        public string Domain { get; set; }
    }
}
