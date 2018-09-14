using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OleDb;
using System.Threading.Tasks;

namespace POMapping
{
    class POMapping
    {
        static void Main(string[] args)
        {
            var strResponse = "";
            POMappingAPIAccess objPOMappingAPIAccess = new POMappingAPIAccess();
            POMappingDAL objPOMappingDAL = new POMappingDAL();
            strResponse = objPOMappingAPIAccess.postPOMappingData();
            if (strResponse == "SUCCESSFUL")
            {
                objPOMappingDAL.UpdatePOMappingData();
            }

        }


    }


}
