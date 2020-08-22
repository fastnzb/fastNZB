using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack;
using FastNZB.ServiceModel.Types;

namespace FastNZB.ServiceModel
{
    [Route("/api/users/passwordreset/{Id}")]
    [Route("/api/users/passwordreset")]
    public class PasswordResetRequest
    {
        public string Email { get; set; }
        public string Id { get; set; }
        public string NewPassword { get; set; }
    }

    public class PasswordResetResponse
    {
        public string Id { get; set; }
        public bool PasswordChanged { get; set; }
        public bool Valid { get; set; }
    }

}
