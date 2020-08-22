using System;
using ServiceStack;
using ServiceStack.Auth;
using FastNZB.ServiceModel;
using FastNZB.ServiceModel.Types;
using FastNZB;
using System.Data;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using ServiceStack.Configuration;

namespace FastNZB.ServiceInterface
{    
    public class UserServices : Service
    {
        public IAuthRepository AuthRepo { get; set; }

        //Called when a password reset link is clicked.
        public object Get(PasswordResetRequest request)
        {
            //Display Change Password Screen
            var resetrequest = Cache.Get<PasswordResetRequest>(request.Id);

            var response = new PasswordResetResponse();
            response.Valid = !(resetrequest == null);
            response.Id = request.Id;

            return response;
        }

        //Called when the password request is initiated.
        public object Post(PasswordResetRequest request)
        {
            if (request.Email == null)
            {
                //this.Response.StatusCode = 400;
                return new HttpError(400, "you must provide an username.");
            }

            if (AuthRepo.GetUserAuthByUserName(request.Email) == null)
            {
                //this.Response.StatusCode = 400;
                return new HttpError(400, "user not registered.");
            }

            request = ((FastNZBOrmLiteAuthRepository)AuthRepo).ResetPassword(request, AuthRepo.GetUserAuthByUserName(request.Email));
            
            Cache.Add<PasswordResetRequest>(request.Id, request, new TimeSpan(1, 0, 0));
            
            return new HttpResult("an email has been sent with a link to reset your password.");
        }

        public PasswordResetResponse Put(PasswordResetRequest request)
        {
            // VALIDATE

            //Changes the password
            var resetrequest = Cache.Get<PasswordResetRequest>(request.Id);

            var response = new PasswordResetResponse();
            if (resetrequest == null)
             {
                response.Valid = false;
                return response;
             }
            if (request.Email != resetrequest.Email)
            {
                response.Valid = false;
                return response;
            }
            else if (resetrequest == null)
            {
                response.Valid = false;
                return response;
            }
            else
            {
                response.Valid = true;
            }

            var existingUser = AuthRepo.GetUserAuthByUserName(resetrequest.Email);
            if (existingUser == null)
            {
                return new PasswordResetResponse() { Valid = false };
            }

            AuthRepo.UpdateUserAuth(existingUser, existingUser, request.NewPassword);

            response.PasswordChanged = true;
            Cache.Remove(resetrequest.Id);
            return response;
        }
    }
}
