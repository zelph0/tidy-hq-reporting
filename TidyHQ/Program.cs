using System;
using System.Collections.Generic;
using System.IO;
using TidyHQ.Client;
using TidyHQ.Resources;

namespace TidyHQ
{
    public class Program
    {
        private static string ClientId { get; } = "";
        private static string ClientSecret { get; } = "";
        private static Uri RestApiUri { get; } = new Uri("https://api.tidyhq.com/v1/");
        private static Uri TokenRequestUri { get; } = new Uri("https://accounts.tidyhq.com/oauth/token");
        private static string Username { get; } = ""; //TODO: Enter your username - this must match all domain/organizations you're using
        private static string Password { get; } = ""; //TODO: Enter your password here - this must match all domain/organizations you're using

        private static ConnectClient _connectClient;

        private static List<string> groupInformation = new List<string>();
        private static List<string> contactInformation = new List<string>();

        static void Main(string[] args) {
            var domainPrefix = new[] {
                
               new [] {"domain"}

                //TODO: To add multiple groups/organizations in parrelel add them here
           };
            for (int i = 0; i < domainPrefix.GetLength(0); i++) {
                _connectClient = new ConnectClient(TokenRequestUri, RestApiUri, domainPrefix[i][0].ToString(), ClientId, ClientSecret, Username, Password);
                var accessToken = _connectClient.TokenRequest();
                var contactsList = _connectClient.Contacts(accessToken.access_token);
                foreach (var contact in contactsList) {
                    contactInformation.Add($"{domainPrefix[i][0]},{contact.last_name},{contact.first_name},{contact.address1},{contact.phone_number},{contact.email_address}");//Where you include more data
                    var groupsList = _connectClient.Groups(accessToken.access_token, contact.id);
                    foreach (var group in groupsList) {
                        groupInformation.Add($"{group.label},{contact.first_name} {contact.last_name},{domainPrefix[i][0]}");
                    }
                }
                WriteFile(domainPrefix[i][0], groupInformation, contactInformation);
            }
            Console.WriteLine("Finished - Press Enter to Exit");
            Console.ReadLine();
        }

        private static void WriteFile(string domainPrefix, List<string> groupInformation, List<string> contactInformation )
        {
            var dir = "C:\\output";
            var contactFilePath = Path.Combine(dir, "Contact_Data.csv");
            var groupFilePath = Path.Combine(dir, "Group_Data.csv");
            using (var tw = File.CreateText(contactFilePath)) {
                tw.WriteLine("domain,First Name,Last Name");
                foreach (var contact in contactInformation) {
                    tw.WriteLine(contact);
                    //tw.WriteLine("{0},{1},{2},{3},{4}", contact.id, contact.first_name, contact.last_name, contact.phone_number, contact.email_address);
                }
            }
            using (var tw = File.CreateText(groupFilePath)) {
                tw.WriteLine("Group,Name,FEC");
                foreach (var group in groupInformation) {
                    tw.WriteLine(group);
                }
            }
        }
    }
}

