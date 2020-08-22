using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

using ServiceStack;
using ServiceStack.Data;
using ServiceStack.Auth;
using ServiceStack.OrmLite;
using ServiceStack.Configuration;

using MailKit.Net.Smtp;
using MimeKit;

using FastNZB.ServiceModel.Types;
using FastNZB.ServiceModel;

namespace FastNZB
{
    public class FastNZBOrmLiteAuthRepository
        : OrmLiteAuthRepository<UserAuth, UserAuthDetails>, IUserAuthRepository
    {
        protected IDbConnectionFactory dbFactory { get; set; }

        public FastNZBOrmLiteAuthRepository(IDbConnectionFactory dbFactory)
            : base(dbFactory)
        {
            this.dbFactory = dbFactory;
        }

        public override IUserAuth CreateUserAuth(IUserAuth newUser, string password)
        {
            var user = base.CreateUserAuth(newUser, password);

            using (var db = dbFactory.OpenDbConnection())
            {
                var settings = new TextFileSettings("appsettings.txt");
                var provider = new ApiKeyAuthProvider();
                provider.KeySizeBytes = 32;
                var key = provider.GenerateApiKey(String.Empty, String.Empty, 32).Substring(0, 32);
                db.Save<APIKey>(new APIKey()
                {
                    UserId = user.Id,
                    RequestLimit = 2500,
                    Key = key
                });

                var mimeMessage = new MimeMessage();
                mimeMessage.From.Add(new MailboxAddress(settings.GetString("EmailFromName"), settings.GetString("EmailFrom")));
                mimeMessage.To.Add(new MailboxAddress(user.Email, user.Email));
                mimeMessage.Subject = "Your api key";
                var builder = new BodyBuilder();
                builder.TextBody =
                        "Your api key is: " + key +
                            "\n\n" +
                            "Support: " + settings.GetString("EmailFrom");
                builder.HtmlBody =
                         "Your api key is: " + key +
                             "<br /><br />" +
                             "Support: " + settings.GetString("EmailFrom");

                mimeMessage.Body = builder.ToMessageBody();
                using (var client = new SmtpClient())
                {

                    client.Connect(settings.GetString("SMTPHost"), 587, false);
                    // Note: only needed if the SMTP server requires authentication
                    // Error 5.5.1 Authentication 
                    client.Authenticate(settings.GetString("SMTPUser"), settings.GetString("SMTPPass"));
                    client.Send(mimeMessage);
                    client.Disconnect(true);

                }
            }

            return user;
        }

        public PasswordResetRequest ResetPassword(PasswordResetRequest request, IUserAuth user)
        {
            request.Id = Guid.NewGuid().ToString().Replace("-", "");
            var settings = new TextFileSettings("appsettings.txt");
            string subject = "Password Reset Requested";
            string html = String.Format(String.Concat(
                "Hello {0},",
                "<br /><br />",
                "You recently requested to change your password at <a href=\"{1}\">{1}</a>",
                "<br/><br/>",
                "You may use the following link to set your passowrd: {2}",
                "<br/><br/>",
                "This link will be valid for 1 hour"),
                user.UserName,
                settings.Get<string>("BaseUrl"),
                String.Format("<a href=\"{0}/reset/{1}\">{0}/reset/{1}</a>", settings.Get<string>("BaseUrl"), request.Id)
            );

            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress(settings.GetString("EmailFromName"), settings.GetString("EmailFrom")));
            mimeMessage.To.Add(new MailboxAddress(user.Email, user.Email));
            mimeMessage.Subject = subject;
            var builder = new BodyBuilder();
            builder.HtmlBody = html;
            mimeMessage.Body = builder.ToMessageBody();
            using (var client = new SmtpClient())
            {

                client.Connect(settings.GetString("SMTPHost"), 587, false);
                // Note: only needed if the SMTP server requires authentication
                // Error 5.5.1 Authentication 
                client.Authenticate(settings.GetString("SMTPUser"), settings.GetString("SMTPPass"));
                client.Send(mimeMessage);
                client.Disconnect(true);

            }
            return request;
        }

    }

}