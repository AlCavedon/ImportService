using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ImportService.BusinessLogic.CsvFileReaderClass;
using ImportService.Objects;

namespace ImportService.BusinessLogic
{
    class BusinessLogic
    {
        private static void writelog(string _message)
        {
            using (StreamWriter log = new StreamWriter(@"c:\Logs\UPSImport.log", true))
            {
                log.WriteLine("{0} -- {1}", DateTime.Now.ToString(), _message);
                log.Flush();
            }
        }

        public static void ImportDataFedEx(String fileName, String fullPath)
        {
            //ALS: no db...
            //ClientsDataClassesDataContext clientdb = new ClientsDataClassesDataContext();
            //FreightBillDataClassesDataContext fbdb = new FreightBillDataClassesDataContext();

            //initialize tracking number (column 6) and loop until the number changes
            String FBInvoiceNumber = String.Empty;
            String TrackingNumber = String.Empty;

            // get total from the K column - used for validation - it equals the total invoice amount
            Decimal TotalInvoice = 0;
            Decimal TotalInvoiceValidate = 0;

            //calulate this Fridays date 
            var today = DateTime.Today;
            var startDay = ((int)today.DayOfWeek);
            DateTime thisFriday = today.AddDays(5 - startDay);

            DateTime? WeekendingDate = thisFriday;
            DateTime? BatchDate = today;  //Maxine 9/9/2015 changed the batch date from friday to date of import
            DateTime? InvoiceDate = thisFriday;
            DateTime DueDate = thisFriday;
            String InvoiceNumber = String.Format("{0}HOP10", thisFriday.ToString("MMddyy")); //.Replace("/", ""));

            // init the variables
            String RecipentNumber = String.Empty;
            String AccountNumber = String.Empty;
            String InvTypeCode = String.Empty;
            String InvDetailCode = String.Empty;
            String ProDate = String.Empty;
            String Ref1 = String.Empty;
            String Ref2 = String.Empty;
            String Ref3 = String.Empty;
            String PayCode = String.Empty;
            int Pieces = 0;
            String ProNumber = String.Empty;
            int ActualWeight = 0;
            int BilledWeight = 0;
            String ChargeClassCode = String.Empty;
            String ProTransNote = String.Empty;
            Decimal PayAmount = 0;
            String ShipperName = String.Empty;
            String ShipperAddress1 = String.Empty;
            String ShipperAddress2 = String.Empty;
            String ShipperCity = String.Empty;
            String ShipperState = String.Empty;
            String ShipperZip = String.Empty;
            String ShipperCountry = String.Empty;
            String ConsigneeName = String.Empty;
            String ConsigneeAddress1 = String.Empty;
            String ConsigneeAddress2 = String.Empty;
            String ConsigneeCity = String.Empty;
            String ConsigneeState = String.Empty;
            String ConsigneeZip = String.Empty;
            String ConsigneeCountry = String.Empty;
            Decimal FSC = 0;
            String LoadCode = String.Empty;
            String ErrorCode = String.Empty;
            String Status = String.Empty;
            String GLCode = String.Empty;
            String SLCode = String.Empty;
            String ServiceLevel = String.Empty;

            //fullPath = String.Format("{0}{1}", fullPath, fileName);
            CsvFileClass.CsvFile[] CsvFile;
            var f = File.ReadAllLines(fullPath);

            int lCount = f.Length;
            CsvFile = new CsvFileClass.CsvFile[lCount];

            using (CsvFileReader reader = new CsvFileReader(fullPath))
            {
                bool hasRow = true;
                int i = 0;

                TotalInvoiceValidate = 0;

                CsvRow row = new CsvRow();
                if (row.Count() == 0)
                {
                    reader.ReadRow(row);
                }

                else
                {
                    reader.ReadRow(row);
                }

                while (hasRow)
                {
                    CsvFile[i] = new CsvFileClass.CsvFile
                    {
                        RecipentNumber = Convert.ToString(row[1]),
                        AccountNumber = Convert.ToString(row[2]),
                        //InvTypeCode = Convert.ToString(row[6]),
                        //InvDetailCode = Convert.ToString(row[7]),
                        ProDate = Convert.ToString(row[14]),
                        Ref1 = Convert.ToString(row[49]),
                        Ref2 = Convert.ToString(row[50]),
                        Ref3 =  Convert.ToString(row[51]),
                        PayCode = Convert.ToString(row[17]),
                        Pieces = Convert.ToString(row[18]),
                        ProNumber = Convert.ToString(row[20]),
                        ActualWeight = Convert.ToString(row[26].ToString()),
                        BilledWeight = Convert.ToString(row[28].ToString()),
                        ChargeClassCode = Convert.ToString(row[43]),
                        ProTransNote = Convert.ToString(row[45]),
                        FSCCheck = Convert.ToString(row[43]),
                        PayAmount = Convert.ToString(row[52].ToString()),
                        ShipperName = Convert.ToString(row[67]),
                        ShipperAddress1 = Convert.ToString(row[68]),
                        ShipperAddress2 = Convert.ToString(row[69]),
                        ShipperCity = Convert.ToString(row[70]),
                        ShipperState = Convert.ToString(row[71]),
                        ShipperZip = Convert.ToString(row[72]),
                        ShipperCountry = Convert.ToString(row[73]),
                        ConsigneeName = Convert.ToString(row[75]),
                        ConsigneeAddress1 = Convert.ToString(row[76]),
                        ConsigneeAddress2 = Convert.ToString(row[77]),
                        ConsigneeCity = Convert.ToString(row[78]),
                        ConsigneeState = Convert.ToString(row[79]),
                        ConsigneeZip = Convert.ToString(row[80]),
                        ErrorCodeCheck = Convert.ToString(row[2]),
                        PaymentType = Convert.ToString(row[17]),
                        TotalInvoice = Convert.ToString(row[10]),
                        FBInvoiceNumber = Convert.ToString(row[5])

                    };
                    i++;

                    //get the next row

                    hasRow = reader.ReadRow(row);
                }

            }

            CsvFile = CsvFile.AsEnumerable().OrderBy(l => l.FBInvoiceNumber).ThenBy(l => l.ProNumber).ToArray();

            TotalInvoiceValidate = 0;
            for (var i = 0; i < CsvFile.Length; i++)
            {

                if (TrackingNumber != CsvFile[i].ProNumber)

                {
                    TrackingNumber = CsvFile[i].ProNumber;
                    //new tracking number reset variables
                    PayAmount = 0;
                    ActualWeight = 0;
                    BilledWeight = 0;
                    Pieces = 0;
                    FSC = 0;
                }


                if (TotalInvoice != TotalInvoiceValidate && FBInvoiceNumber != CsvFile[i].FBInvoiceNumber)
                {
                    writelog(String.Format("VALIDATION ERROR! Total Invoice {0} is not equal to all of the records {1} for Invoice Number {2}", TotalInvoice, TotalInvoiceValidate, FBInvoiceNumber));

                }

                TotalInvoice = Convert.ToDecimal(CsvFile[i].TotalInvoice);
                if (FBInvoiceNumber != CsvFile[i].FBInvoiceNumber)
                {
                    FBInvoiceNumber = CsvFile[i].FBInvoiceNumber;
                    TotalInvoiceValidate = 0;

                }

                while (i < CsvFile.Length && TrackingNumber == CsvFile[i].ProNumber && FBInvoiceNumber == CsvFile[i].FBInvoiceNumber)
                {
                    RecipentNumber = CsvFile[i].RecipentNumber;
                    AccountNumber = CsvFile[i].AccountNumber;
                    InvTypeCode = CsvFile[i].InvTypeCode;
                    InvDetailCode = CsvFile[i].InvDetailCode;
                    ProDate = CsvFile[i].ProDate;
                    Ref1 = CsvFile[i].Ref1;
                    Ref2 = CsvFile[i].Ref2;
                    PayCode = CsvFile[i].PayCode;
                    int result = 0;
                    if (int.TryParse(CsvFile[i].Pieces, out result))
                    {
                        Pieces += Convert.ToInt32(CsvFile[i].Pieces);
                    }
                    if (TrackingNumber == String.Empty)
                    {
                        ProNumber = FBInvoiceNumber;
                    }
                    else
                    {
                        ProNumber = CsvFile[i].ProNumber;
                    }

                    if (int.TryParse(CsvFile[i].ActualWeight, out result))
                    {
                        ActualWeight = Convert.ToInt32(CsvFile[i].ActualWeight);
                    }
                    result = 0;
                    if (int.TryParse(CsvFile[i].BilledWeight, out result))
                    {
                        BilledWeight = Convert.ToInt32(CsvFile[i].BilledWeight);
                    }
                    ChargeClassCode = CsvFile[i].ChargeClassCode;
                    ProTransNote = CsvFile[i].ProTransNote;
                    if (CsvFile[i].FSCCheck == "FSC")
                    {
                        FSC += Convert.ToDecimal(CsvFile[i].PayAmount);
                        PayAmount += Convert.ToDecimal(CsvFile[i].PayAmount);
                    }
                    else
                    {
                        PayAmount += Convert.ToDecimal(CsvFile[i].PayAmount);
                    }

                    ShipperName = CsvFile[i].ShipperName;
                    ShipperAddress1 = CsvFile[i].ShipperAddress1;
                    ShipperAddress2 = CsvFile[i].ShipperAddress2;
                    ShipperCity = CsvFile[i].ShipperCity;
                    ShipperState = CsvFile[i].ShipperState;
                    if (CsvFile[i].ShipperZip.Length > 5)
                    {
                        ShipperZip = CsvFile[i].ShipperZip.Substring(0, 4);
                    }
                    else
                    {
                        ShipperZip = CsvFile[i].ShipperZip;
                    }
                    ShipperCountry = CsvFile[i].ShipperCountry;
                    ConsigneeName = CsvFile[i].ConsigneeName;
                    ConsigneeAddress1 = CsvFile[i].ConsigneeAddress1;
                    ConsigneeAddress2 = CsvFile[i].ConsigneeAddress2;
                    ConsigneeCity = CsvFile[i].ConsigneeCity;
                    ConsigneeState = CsvFile[i].ConsigneeState;
                    if (CsvFile[i].ConsigneeZip.Length > 5)
                    {
                        ConsigneeZip = CsvFile[i].ConsigneeZip.Substring(0, 4);
                    }
                    else
                    {
                        ConsigneeZip = CsvFile[i].ConsigneeZip;
                    }
                    ConsigneeCountry = CsvFile[i].ConsigneeCountry;
                    Status = "O";
                    //apply the rules
                    if ((ConsigneeName != null && ConsigneeCity != null & ConsigneeZip != null) && ConsigneeState == null)
                    {
                        LoadCode = "INT";
                    }
                    else if ((ShipperName != null && ShipperCity != null & ShipperZip != null) && ShipperState == null)
                    {
                        LoadCode = "INT";
                    }
                    else
                    {
                        LoadCode = "P";
                    }
                    if (CsvFile[i].AccountNumber == null)
                    {
                        ErrorCode = "J";

                    }
                    else if (CheckForDupePro(CsvFile[i].ProNumber))
                    {
                        ErrorCode = "J";
                        if (CsvFile[i].FSCCheck == "FRT")
                        {
                            ProTransNote = String.Format("DUPLICATE PRO {0}", CsvFile[i].ProTransNote);
                        }
                        else if (CsvFile[i].FSCCheck == "FRT" && ProTransNote == null)
                        {
                            ProTransNote = CsvFile[i].ProTransNote;
                        }
                    }
                    else if (ProNumber == null && CheckForDupePro(CsvFile[i].FBInvoiceNumber))
                    {
                        ErrorCode = "J";
                        if (CsvFile[i].FSCCheck == "FRT")
                        {
                            ProTransNote = String.Format("DUPLICATE PRO_INVOICE_ {0}", CsvFile[i].ProTransNote);
                        }
                        else if (CsvFile[i].FSCCheck == "FRT" && ProTransNote == null)
                        {
                            ProTransNote = CsvFile[i].ProTransNote;
                        }
                    }
                    else
                    {
                        ErrorCode = "A";
                    }

                    GLCode = QueryGLInformation(ProNumber);

                    i++;

                    ////get the next row
                    //row = ds.Tables[0].Rows[i];
                    ////hasRow = reader.ReadRow(row);

                }
                TotalInvoiceValidate += PayAmount;
                // dropped from the while - update index
                i--;
                // create the freight bill
                //get the clientid
                var id = (from c in clientdb.Clients
                          where c.Code == "HOP10"
                          select c.ClientsId).FirstOrDefault();

                if (id != Guid.Empty && ProNumber != "")
                {
                    writelog(String.Format("adding: {0}, weekending date {1} ", FBInvoiceNumber, WeekendingDate));


                    //id is the ClientsId 

                    //get the CarrierScac
                    //CarrierSCACsId
                    int CarrierSCACsId = GetCarrierSCACsId("UPSI");
                    //get shipper addressid or GetShippersIdreate the address and return the id
                    //ShippersId
                    int ShippersId = GetShippersId(id, ShipperName, ShipperCity, ShipperState, ShipperZip);
                    //get consignees addressid or create the address and return the id;
                    //ConsigneesId
                    int ConsigneesId = GetConsigneesId(id, ConsigneeName, ConsigneeCity, ConsigneeState, ConsigneeZip);

                    //using TBill.Load Code - get the load code id to store in FB
                    //Load Code
                    int LoadCodeId = GetLoadCodeId(LoadCode);
                    //using TBill.Create Payment recordent Type - get the PaymentType key
                    //PaymentTypesId
                    int PaymentTypesId = GetPaymentTypeId(CsvFile[i].PaymentType);
                    //using TBill.Error Code to get the InvoiceStatuses key
                    //InvoiceStatusesId
                    int InvoiceStatusesId = GetInvoiceStatusesId(ErrorCode);

                    try
                    {
                        Invoices invoice = new Invoices()
                        {
                            WeekendingDate = ErrorCode == "J" ? null : WeekendingDate,
                            BatchDate = ErrorCode == "J" ? null : BatchDate,
                            ProNumber = ProNumber,
                            InvoiceNumber = FBInvoiceNumber,
                            ProDate = Convert.ToDateTime(ProDate),
                            CarrierSCACsId = CarrierSCACsId,
                            ConsigneesId = ConsigneesId,
                            ClientsId = id,
                            InvoiceStatusId = InvoiceStatusesId,
                            LoadsId = LoadCodeId,
                            EmployeesId = 51,
                            PaymentTypesId = PaymentTypesId,
                            ShippersId = ShippersId,
                            GeneralLedgerNumber = QueryGLInformation(ProNumber),
                            ActualWeight = ActualWeight,
                            BilledWeight = BilledWeight,
                            PaymentAmount = PayAmount,
                            BilledAmount = PayAmount,
                            Status = Status,

                            FuelSurcharge = FSC,
                            //Maxine - 8/20/2013 - default to 1 per Al
                            PieceCount = Pieces == 0 ? 1 : Convert.ToInt32(Pieces),
                            //PalletCount = 1,
                            //DeliveryDate = bill.Delivery_Date == null ? (Nullable<DateTime>)null : bill.Delivery_Date,
                            //CreatedBy = CreateById,
                            CreatedDate = DateTime.Today,
                            //UpdatedBy = UpdatedById,
                            //UpdatedDate = bill.PRO_Update_Date_Time == null ? (Nullable<DateTime>)null : bill.PRO_Trans_Update_DAte_Time,

                            Deleted = false,
                            //PreferredCarrierSCAC = bill.Preferred_Carrier_SCAC  == null ? null : bill.Preferred_Carrier_SCAC,
                            //PreferredCarrierAmount = bill.Preferred_Carrier_Amount  == null ? 0 : (decimal)bill.Preferred_Carrier_Amount
                        };
                        fbdb.Invoices.InsertOnSubmit(invoice);
                        fbdb.SubmitChanges();

                        //now create the support/details table since we have the invoiceid
                        bool success = false;
                        //using TBill CTL Invoice Number to create a new record in the InvoiceDetails table
                        //this record needs the InvoicesId 
                        if (ErrorCode == "J")
                        {
                            success = CreateInvoiceDetailsRecord(invoice.InvoicesId, null, null, null);
                        }
                        else
                        {
                            success = CreateInvoiceDetailsRecord(invoice.InvoicesId, InvoiceNumber, InvoiceDate, DueDate);
                        }
                        //using TBill.Reference1,2,3 create a record in the ReferencesDetails table
                        ////this record needs the InvoicesId 
                        if (Ref1 != null || Ref2 != null)
                        {
                            success = CreateReferencesDetailsRecord(invoice.InvoicesId, Ref1, Ref2);
                        }
                        ////using TBill BIDReason - create a record in the BillsInDisputeDetails table
                        ////this record needs the InvoicesId 
                        //if (BIDReason != null)
                        //{
                        //    int BillInDisputeReasonsId = GetBillInDisputeReasonsId(BIDReason);
                        //    success = CreateBillsInDisputeDReasonRecord(invoice.InvoicesId, BillInDisputeReasonsId, bill.BIDReasonOther, bill.BIDOrgAmt, bill.BIDSBAmt, bill.BIDActualWeight);
                        //}
                        //using bill.class code to get the FreightClassId from the FreightClasses table 
                        //this record needs the InvoicesId to relate back to the FB
                        //if (bill.Class_Code != null)
                        //{
                        //    int FreightClassId = GetFreightClassId(ClassCode);
                        //    success = CreateInvoice_FreightClassesRecord(invoice.InvoicesId, FreightClassId, ActualWeight, BilledWeight, Pieces);
                        //}
                        //using TBill.Pro Transaction Note as a key to ProTransNote table to get the key - create a ProTransNotesDetail record - need InvoicesId
                        //NOTE: is no matches then put the Pro Transaction Note in the Note column of the new record
                        //ProTransNotesDetailsId - this record needs the InvoicesId 
                        if (ProTransNote != null)
                        {
                            int ProTransNoteId = 0;
                            ProTransNoteId = GetProTransNoteId(ProTransNote);

                            //if (ProTransNoteId > 0)
                            success = CreateProTansNoteDetailsRecord(invoice.InvoicesId, ProTransNoteId, ProTransNote);
                        }


                    } //ends try
                    catch (Exception ex)
                    {
                        writelog(String.Format("error! creating invoice pro number {0}", ProNumber));

                        writelog(ex.Message);
                        //log.Close();

                    }


                }
                //}// end while
            } //end using
            //move the file to the processed folder
            File.Move(fullPath, String.Format(ConfigHelper.FetchStringValue("PostProcessFolder", true), fileName));
        }
        public static String QueryGLInformation(String billOfLading)
        {

            ClientsDataClassesDataContext clientdb = new ClientsDataClassesDataContext();
            HopesDataClassesDataContext hdb = new HopesDataClassesDataContext();

            Hopes hope = null; //init

            hope = (from Hopes h in hdb.Hopes.ToArray<Hopes>()
                    where h.Waybill == billOfLading
                    select h).FirstOrDefault<Hopes>();

            if (hope == null)
            {
                return String.Empty;
            }
            else
            {
                return hope.GeneralLedgerCode;
            }




        }   // end QueryBillOfLadingInformation()

        private static bool CreateAccessorialRecord(int id, decimal? total)
        {
            try
            {
                //current system does not contain the accessorial code - so we cannot save it
                using (FreightBillDataClassesDataContext fbdb = new FreightBillDataClassesDataContext())
                {
                    AccessorialsDetails acc = new AccessorialsDetails()
                    {
                        InvoicesId = id,
                        AccessorialCharge = Convert.ToDecimal(total)
                    };
                    fbdb.AccessorialsDetails.InsertOnSubmit(acc);
                    fbdb.SubmitChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                writelog(String.Format("error! CreateAccessorialRecord {0}", id));

                writelog(ex.Message);
                return false;
            }

        }
        private static bool CheckForDupePro(String ProNum)
        {
            try
            {
                //current system does not contain the accessorial code - so we cannot save it
                using (FreightBillDataClassesDataContext fbdb = new FreightBillDataClassesDataContext())
                {
                    var pro = from p in fbdb.Invoices
                              where p.ProNumber == ProNum
                              select p.ProNumber;
                    if (pro.Count() > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

            }
            catch (Exception ex)
            {
                writelog(String.Format("error! CreateAccessorialRecord {0}", ProNum));

                writelog(ex.Message);
                return true;
            }

        }

        private static bool CreateProTansNoteDetailsRecord(int InvoicesId, int ProTransNoteId, string PRO_Transaction_Note)
        {
            try
            {
                using (FreightBillDataClassesDataContext fbdb = new FreightBillDataClassesDataContext())
                {
                    ProTransNoteDetails detail = new ProTransNoteDetails()
                    {
                        InvoicesId = InvoicesId,
                        ProTransNotesId = ProTransNoteId,
                        Notes = PRO_Transaction_Note
                    };
                    fbdb.ProTransNoteDetails.InsertOnSubmit(detail);
                    fbdb.SubmitChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                writelog(String.Format("error! CreateProTansNoteDetailsRecord {0}", InvoicesId));

                writelog(ex.Message);
                return false;
            }
        }
        private static bool CreateInvoice_FreightClassesRecord(int InvoicesId, int FreightClassId, int? Actual_Weight, int? Bill_Weight, double? Piece)
        {
            try
            {
                using (FreightBillDataClassesDataContext fbdb = new FreightBillDataClassesDataContext())
                {
                    if (Actual_Weight == null)
                        Actual_Weight = 0;
                    if (Bill_Weight == null)
                        Bill_Weight = 0;
                    FreightClassesDetails fclass = new FreightClassesDetails()
                    {
                        InvoicesId = InvoicesId,
                        FreightClassId = FreightClassId,
                        Weight = (int)Actual_Weight,
                        BilledWeight = Bill_Weight

                    };
                    fbdb.FreightClassesDetails.InsertOnSubmit(fclass);
                    fbdb.SubmitChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                writelog(String.Format("error! CreateInvoice_FreightClassesRecord {0}", InvoicesId));

                writelog(ex.Message);
                return false;
            }
        }
        private static bool CreateBillsInDisputeDReasonRecord(int InvoicesId, int BillInDisputeReasonsId, string BIDReasonOther, decimal? BIDOrgAmt, decimal? BIDSBAmt, int? BIDActualWeight)
        {
            try
            {
                using (FreightBillDataClassesDataContext fbdb = new FreightBillDataClassesDataContext())
                {
                    BillInDisputeDetails reason = new BillInDisputeDetails()
                    {
                        InvoicesId = InvoicesId,
                        BillsInDisputeReasonsId = BillInDisputeReasonsId,
                        Notes = BIDReasonOther,
                        OriginalAmount = BIDOrgAmt,
                        ActualAmount = BIDSBAmt,
                        ActualWeight = BIDActualWeight
                    };
                    fbdb.BillInDisputeDetails.InsertOnSubmit(reason);
                    fbdb.SubmitChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                writelog(String.Format("error! CreateBillsInDisputeDReasonRecord {0}", InvoicesId));

                writelog(ex.Message);
                return false;
            }
        }
        private static bool CreateReferencesDetailsRecord(int InvoicesId, string Reference1, string Reference2)
        {
            try
            {
                using (FreightBillDataClassesDataContext fbdb = new FreightBillDataClassesDataContext())
                {
                    ReferenceDetails details = new ReferenceDetails()
                    {
                        InvoicesId = InvoicesId,
                        ReferenceOne = Reference1,
                        ReferenceTwo = Reference2

                    };
                    fbdb.ReferenceDetails.InsertOnSubmit(details);
                    fbdb.SubmitChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                writelog(String.Format("error! CreateReferencesDetailsRecord {0}", InvoicesId));

                writelog(ex.Message);
                return false;
            }
        }
        private static bool CreateInvoiceDetailsRecord(int InvoicesId, string InvoiceNumber, DateTime? InvoiceDate, DateTime? DueDate)
        {
            try
            {
                using (FreightBillDataClassesDataContext fbdb = new FreightBillDataClassesDataContext())
                {
                    InvoiceDetails detail = new InvoiceDetails()
                    {
                        InvoicesId = InvoicesId,
                        InvoiceNumber = InvoiceNumber,
                        InvoiceDate = InvoiceDate,
                        DueDate = DueDate
                    };
                    fbdb.InvoiceDetails.InsertOnSubmit(detail);
                    fbdb.SubmitChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                writelog(String.Format("error! CreateInvoiceDetailsRecord {0}", InvoiceNumber));

                writelog(ex.Message);
                return false;
            }
        }


        private static int GetBillInDisputeReasonsId(string reason)
        {
            using (FreightBillDataClassesDataContext fbdb = new FreightBillDataClassesDataContext())
            {
                int id = (from r in fbdb.BillInDisputeReasons
                          where r.Reason == reason
                          select r.ReasonsId).FirstOrDefault();
                return id;
            }
        }
        private static int GetProTransNoteId(string note)
        {
            int index = 0;
            string searchstring = String.Empty;
            try
            {
                index = note.IndexOf("~");
                if (index < 1)
                {
                    index = note.IndexOf("ATTACHED") + 7;
                }
                if (index > 0)
                {
                    searchstring = note.Substring(0, index);
                }
                else
                {
                    searchstring = note;
                }
                using (FreightBillDataClassesDataContext fbdb = new FreightBillDataClassesDataContext())
                {
                    int id = (from n in fbdb.ProTransNotes
                              where n.Note.Contains(searchstring)
                              select n.ProTransNoteId).FirstOrDefault();
                    return id;
                }
            }
            catch (Exception ex)
            {
                writelog(String.Format("error! GetProTransNoteId {0}", ex.Message));

                writelog(ex.Message);
                return 0;
            }
        }
        private static int GetInvoiceStatusesId(string code)
        {
            using (FreightBillDataClassesDataContext fbdb = new FreightBillDataClassesDataContext())
            {
                int id = (from c in fbdb.InvoiceStatuses
                          where c.Code == code
                          select c.InvoiceStatusesId).FirstOrDefault();
                return id;
            }
        }
        private static int GetPaymentTypeId(string type)
        {
            //get the CW type from the translations table 
            UPSCodeDataClassesDataContext upsdb = new UPSCodeDataClassesDataContext();
            var Ptype = (from p in upsdb.UPSCodeTranslations
                         where p.UPSCode == type
                         select p.CWPaymentType).SingleOrDefault();
            using (FreightBillDataClassesDataContext fbdb = new FreightBillDataClassesDataContext())
            {
                int id = (from t in fbdb.PaymentTypes
                          where t.Type == Ptype
                          select t.PaymentTypesId).FirstOrDefault();
                return id;
            }
        }
        private static int GetLoadCodeId(string code)
        {
            //if (code == "L")
            //    code = "LTL";
            using (FreightBillDataClassesDataContext fbdb = new FreightBillDataClassesDataContext())
            {
                int id = (from l in fbdb.Loads
                          where l.Code == code
                          select l.LoadsId).FirstOrDefault();
                return id;
            }
        }
        private static int GetFreightClassId(double? code)
        {
            using (FreightBillDataClassesDataContext fbdb = new FreightBillDataClassesDataContext())
            {
                int id = (from c in fbdb.FreightClasses
                          where c.Number == code
                          select c.FreightClassId).FirstOrDefault();
                if (id != 0)
                    return id;
                else
                    return 0;
            }
        }
        private static int GetConsigneesId(Guid cid, string name, string city, string state, string zip)
        {
            using (ClientsDataClassesDataContext clientdb = new ClientsDataClassesDataContext())
            {
                int id = (from a in clientdb.ClientAddresses
                          where a.Name == name
                          select a.ClientAddressesId).FirstOrDefault();
                if (id != 0)
                {
                    return id;
                }
                else if (name != null)
                {
                    //create the address and return the id
                    ClientAddresses address = new ClientAddresses
                    {
                        ClientsId = cid,
                        Name = name,
                        City = city,
                        State = state,
                        ZipCode = zip,
                        Active = true,
                        Type = "Consignee"
                    };
                    clientdb.ClientAddresses.InsertOnSubmit(address);
                    clientdb.SubmitChanges();
                    return address.ClientAddressesId;
                }
            }
            return 0;
        }
        private static int GetShippersId(Guid cid, string name, string city, string state, string zip)
        {
            using (ClientsDataClassesDataContext clientdb = new ClientsDataClassesDataContext())
            {
                int id = (from a in clientdb.ClientAddresses
                          where a.Name == name
                          select a.ClientAddressesId).FirstOrDefault();
                if (id != 0)
                {
                    return id;
                }
                else if (name != null)
                {
                    //create the address and return the id
                    ClientAddresses address = new ClientAddresses
                    {
                        ClientsId = cid,
                        Name = name,
                        City = city,
                        State = state,
                        ZipCode = zip,
                        Active = true,
                        Type = "Shipper"
                    };
                    clientdb.ClientAddresses.InsertOnSubmit(address);
                    clientdb.SubmitChanges();
                    return address.ClientAddressesId;
                }
            }
            return 0;
        }
        private static int GetCarrierSCACsId(string code)
        {
            using (FreightBillDataClassesDataContext fbdb = new FreightBillDataClassesDataContext())
            {
                int scac = (from s in fbdb.CarrierSCACs
                            where s.Code == code
                            select s.CarrierSCACsId).FirstOrDefault();
                return scac;
            }
        }



        private int GetBillTypeId(string type)
        {
            switch (type)
            {
                case "M":
                    {
                        return 2;
                    }
                default:
                    {
                        return 1;
                    }

            }
        }

    }
}
