using MicroServiceNet8.DTO.Common;
using System.ServiceModel;

namespace MicroServiceNet8.Helper
{
    public class HelperFault
    {
        public static FaultException<DTOError> BusinessFault(string errorString, string errorMember = "", string errorMessage = "")
        {
            var fault = new DTOError();
            fault.ErrorString = errorString;
            fault.ErrorMember = errorMember;
            fault.ErrorMessage = errorMessage;
            return new FaultException<DTOError>(fault, new FaultReason(errorMessage));
        }

        public static FaultException<DTOError> BusinessFault(Exception ex)
        {
            var fault = new DTOError();
            fault.ErrorCode = 0;
            fault.ErrorMessage = ex.Message;
            return new FaultException<DTOError>(fault, new FaultReason(ex.Message));
        }

        public static FaultException<DTOError> ServiceFault(int? errorCode, string errorMember = "", string errorMessage = "")
        {
            var fault = new DTOError();
            fault.ErrorCode = errorCode;
            fault.ErrorMember = errorMember;
            fault.ErrorMessage = errorMessage;
            return new FaultException<DTOError>(fault, new FaultReason(errorMessage));
        }

        public static FaultException<DTOError> ServiceFault(Exception ex)
        {
            var fault = new DTOError();
            fault.ErrorCode = 0;
            fault.ErrorMessage = ex.Message;
            return new FaultException<DTOError>(fault, new FaultReason(ex.Message));
        }
    }
}
