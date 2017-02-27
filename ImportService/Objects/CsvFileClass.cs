using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportService.Objects
{
    class CsvFileClass
    {
        public class CsvFile
        {
            //set up the variables
            public String RecipentNumber { get; set; }
            public String AccountNumber { get; set; }
            public String InvTypeCode { get; set; }
            public String InvDetailCode { get; set; }
            public String ProDate { get; set; }
            public String Ref1 { get; set; }
            public String Ref2 { get; set; }
            public String PayCode { get; set; }
            public String Pieces { get; set; }
            public String ProNumber { get; set; }
            public String ActualWeight { get; set; }
            public String BilledWeight { get; set; }
            public String ChargeClassCode { get; set; }
            public String ProTransNote { get; set; }
            public String PayAmount { get; set; }
            public String ShipperName { get; set; }
            public String ShipperAddress1 { get; set; }
            public String ShipperAddress2 { get; set; }
            public String ShipperCity { get; set; }
            public String ShipperState { get; set; }
            public String ShipperZip { get; set; }
            public String ShipperCountry { get; set; }
            public String ConsigneeName { get; set; }
            public String ConsigneeAddress1 { get; set; }
            public String ConsigneeAddress2 { get; set; }
            public String ConsigneeCity { get; set; }
            public String ConsigneeState { get; set; }
            public String ConsigneeZip { get; set; }
            public String ConsigneeCountry { get; set; }
            public String FSC { get; set; }
            public String LoadCode { get; set; }
            public String ErrorCode { get; set; }
            public String Status { get; set; }
            public String GLCode { get; set; }
            public String FSCCheck { get; set; }
            public String ErrorCodeCheck { get; set; }
            public String PaymentType { get; set; }
            public String TotalInvoice { get; set; }
            public String FBInvoiceNumber { get; set; }
            public String SLCode { get; set; }
            public String ServiceLevel { get; set; }
        }
    }
}
