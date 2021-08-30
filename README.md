# Email Validation [![Build status](https://ci.appveyor.com/api/projects/status/67ubhtmijuhyhq6q?svg=true)](https://ci.appveyor.com/project/eshohag/Email.Validation) [![NuGet Badge](https://buildstats.info/nuget/Email.Validation)](https://www.nuget.org/packages/Email.Validation)

Email.Validation in C# .NET Standard 2.0 to validate Email- Syntax, Domain/MX record and Handshake with Email server to validate Email addresses information

## Code Syntax
```csharp
using Email.Validation;
namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var emailInfo = EmailInfo.Validation("shohaghassan14@gmail.com");

        }
    }
}

```
