using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Security.Cryptography.X509Certificates;


namespace WMInventoryGoodsMovements
{
    class AlwaysIgnoreCertPolicy : ICertificatePolicy
    {
        public bool CheckValidationResult(ServicePoint srvPoint, X509Certificate cert, WebRequest request, int certificateProblem)
        {
            // return TRUE to force the certificate to be accepted.
            return true;
        }
    }
}
