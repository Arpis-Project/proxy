using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VyVDbNet
{

    public class GenerateDocument
    {
        public string Internal_Id { get; set; }
        public string SupplierId { get; set; }
        public string UserId { get; set; }
        public string DocType { get; set; }
        public string DocInfo { get; set; }

        public string GetJson()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                CheckAdditionalContent = false
            };

            return JsonConvert.SerializeObject(this, settings);

        }
    }




    public class DocumentDbNet
    {
        public DocumentId documentId { get; set; }
        public Supplier supplier { get; set; }
        public Customer customer { get; set; }
        public LegalMonetaryTotal legalMonetaryTotal { get; set; }
        public List<ReferenceList> references { get; set; }
        public Transportation transportation { get; set; }
        public OtherTaxes otherTaxes { get; set; }
        public List<PaymentList> paymentList { get; set; }
        public AdditionalValues additionalValues { get; set; }
        public List<Item> Items { get; set; }

        public DocumentDbNet()
        {
            this.Items = new List<Item>();
        }

        public string GetString()
        {



            List<object> document = new List<object>();

            if (this.documentId != null)
            {
                document.Add(this.documentId);
            }

            if (this.supplier != null)
            {
                document.Add(this.supplier);
            }

            if (this.customer != null)
            {
                document.Add(this.customer);
            }

            if (this.legalMonetaryTotal != null)
            {
                document.Add(this.legalMonetaryTotal);
            }

            if (this.Items != null)
            {
                foreach (Item item in this.Items)
                {
                    document.Add(item);
                }
            }

            if (this.additionalValues != null)
            {
                document.Add(this.additionalValues);
            }

            if (this.otherTaxes != null)
            {
                document.Add(this.otherTaxes);
            }


            if (this.transportation != null)
            {
                document.Add(this.transportation);
            }


            if (this.paymentList != null)
            {
                foreach (var payment in this.paymentList)
                {
                    document.Add(payment);
                }
            }

            if (this.references != null)
            {
                foreach (var reference in this.references)
                {
                    document.Add(reference);
                }
            }


            object jsonObjet = new { Document = document.ToArray() };

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                Formatting = Formatting.Indented,
                CheckAdditionalContent = false
            };

            return JsonConvert.SerializeObject(jsonObjet, settings);
        }

        public bool GetBase64(out string base64, out string error)
        {
            base64 = "";
            error = "";

            try
            {
                if (GetString() == null) return false;
                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(GetString());
                base64 = System.Convert.ToBase64String(plainTextBytes);
                return true;
            }
            catch (Exception e)
            {
                error = e.Message;
            }
            return false;
        }
    }


    public class DocumentId
    {

        public DocumentId()
        {
            this.InfoType = "DocumentId";
            this.DocumentIdLine = "1";

            DocType = null;
            IssueDate = null;
            ServiceIndicator = null;
            NetAmountIndicator = null;
            DispatchType = null;
            GoodsTransferType = null;
            PaymentMean = null;
            DueDate = null;
        }

        public string InfoType { get; set; }
        public string DocumentIdLine { get; set; }
        public string DocType { get; set; }
        public string IssueDate { get; set; }
        public string ServiceIndicator { get; set; }
        public string NetAmountIndicator { get; set; }

        public string DispatchType { get; set; }
        public string GoodsTransferType { get; set; }
        public string PaymentMean { get; set; }
        public string DueDate { get; set; }


    }

    public class Supplier
    {
        public Supplier()
        {
            this.InfoType = "Supplier";
            this.SupplierLine = "1";
            this.SupplierPhone = null;
        }

        public string InfoType { get; set; }
        public string SupplierLine { get; set; }
        public string SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string SupplierFax { get; set; }


        public string SupplierPhone { get; set; }
        public string SupplierActivity { get; set; }
        public string SupplierActivityCode { get; set; }
        public string SupplierAddress { get; set; }
        public string SupplierCitySubdivision { get; set; }
        public string SupplierCity { get; set; }
    }


    //CustomerLine Always 1	n1
    //CustomerId  Legal Id of Customer(RUT)  n8-an1
    //CustomerName    Customer comercial name an100
    //CustomerActivity Customer economic activity  an40
    //CustomerAddress Customer comercial address an70
    //CustomerCitySubdivision Name of city subdivision(comuna)   an20


    public class Customer
    {
        public Customer()
        {
            this.InfoType = "Customer";
            this.CustomerLine = "1";
        }

        public string InfoType { get; set; }
        public string CustomerLine { get; set; }
        public string CustomerId { get; set; }

        public string CustomerName { get; set; }
        public string CustomerActivity { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerCitySubdivision { get; set; }


    }

    //TransportationLine Always 1	n1
    //CarrierLegalId  Legal Id of carrier n8-an1
    //DriverLegalId   Legal Id of driver  n8-an1
    //DriverName  Name of driver an30
    //LicensePlate Id of vehicle used for transportation an8
    //DestinationAddress Destination address if different from CustomerAddress an70
    //DestinationCitySubdivision Name of city subdivision(comuna)   an20

    public class Transportation
    {
        public Transportation()
        {
            this.InfoType = "Transportation";
            this.TransportationLine = "1";
        }

        public string InfoType { get; set; }
        public string TransportationLine { get; set; }
        public string CarrierLegalId { get; set; }
        public string DriverLegalId { get; set; }
        public string DriverName { get; set; }
        public string LicensePlate { get; set; }
        public string DestinationAddress { get; set; }
        public string DestinationCitySubdivision { get; set; }

    }

    //LegalMonetaryTotalLine Always 1	n1
    //TaxableAmount   Net amount to wich the tax percent is applied n18
    //ExemptAmount Amount exempt from taxation n18
    //VatPercent Percent of VAT  n3.n2
    //VatAmount   Amount of calculated VAT    n18
    //TotalAmount Monetary total for document n18
    //PayableAmount Monetary total to be paid   n18


    public class LegalMonetaryTotal
    {
        public LegalMonetaryTotal()
        {
            this.InfoType = "LegalMonetaryTotal";
            this.LegalMonetaryTotalLine = "1";
            VatPercent = null;

        }

        public string InfoType { get; set; }
        public string LegalMonetaryTotalLine { get; set; }
        public string TaxableAmount { get; set; }
        public string ExemptAmount { get; set; }

        public string VatPercent { get; set; }
        public string VatAmount { get; set; }
        public string TotalAmount { get; set; }
        public string PayableAmount { get; set; }
    }

    //OtherTaxesLine Number of OtherTaxes line n1
    //TaxCode Code of tax type n4
    //TaxPercent Tha tax rate applied, expressed as a percentage n3.n2
    //TaxAmount   The amount of this tax subtotal n18

    public class OtherTaxes
    {
        public OtherTaxes()
        {
            this.InfoType = "OtherTaxes";
            this.OtherTaxesLine = "1";
        }
        public string InfoType { get; set; }
        public string OtherTaxesLine { get; set; }
        public string TaxCode { get; set; }
        public string TaxPercent { get; set; }
        public string TaxAmount { get; set; }



    }

    //ReferenceListLine Number of ReferenceList line n2
    //ReferenceType Type of Reference   an3
    //ReferenceId Identification of Reference Document    an18
    //ReferenceDate   Reference Document Date YYYY-MM-DD
    //ReferenceReason Explain reason of this reference an90

    public class ReferenceList
    {
        public ReferenceList()
        {
            this.InfoType = "ReferenceList";

        }

        public string InfoType { get; set; }
        public string ReferenceListLine { get; set; }
        public string ReferenceType { get; set; }
        public string ReferenceId { get; set; }
        public string ReferenceCode { get; set; }
        public string ReferenceDate { get; set; }
        //public string ReferenceReason { get; set; }

    }

    //PaymentListLine Number of PaymentList line n2
    //PaymentType Type of payment an20
    //PaymentDate Due date of payment YYYY-MM-DD
    //PaymentAmount   Amount of payment n18
    //DocNumber Identification of payment document an20
    //BankName Bank of the payment document    an40
    //BankOffice  Id of bank office   an40

    public class PaymentList
    {
        public PaymentList()
        {
            this.InfoType = "PaymentList";

        }

        public string InfoType { get; set; }
        public string PaymentListLine { get; set; }
        public string PaymentType { get; set; }
        public string PaymentDate { get; set; }
        public string PaymentAmount { get; set; }
        public string DocNumber { get; set; }
        public string BankName { get; set; }
        public string BankOffice { get; set; }
    }


    public class Item
    {
        public Item()
        {
            this.InfoType = "Item";
        }

        public string InfoType { get; set; }
        public string ItemLine { get; set; }
        public string ItemCodeType { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string LineQuantity { get; set; }
        public string ItemMeasureUnit { get; set; }
        public string ItemPrice { get; set; }
        public string LineAmount { get; set; }
    }


    public class AdditionalValues
    {

        public AdditionalValues()
        {
            this.InfoType = "AdditionalValues";
            this.AdditionalValuesLine = "1";
        }

        public string InfoType { get; set; }
        public string AdditionalValuesLine { get; set; }
        public string Val1 { get; set; }
        public string Val2 { get; set; }
        public string Val3 { get; set; }
        public string Val4 { get; set; }
        public string Val5 { get; set; }
        public string Val6 { get; set; }
        public string Val7 { get; set; }
        public string Val8 { get; set; }
        public string Val9 { get; set; }
    }




}
