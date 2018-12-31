using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Globalization;

namespace ParkingSystemAPI.Controllers
{
    public class ParkingController : ApiController
    {
        System.Data.SqlClient.SqlConnection tmpCon = new System.Data.SqlClient.SqlConnection();
        String saconstring = @"Data Source=.\TRING;Initial Catalog=TES675496797;Persist Security Info=True;User ID=sa; Password=zdxfks83?";
        double price = 1;
        double priceMoreThanFour = 1.5;

        [Route("api/ParkingController/PostParking/{customer}/{operater}")]
        [System.Web.Http.AcceptVerbs("GET", "POST")]
        [System.Web.Http.HttpGet]
        public string PostParking(String customer, String operater)  // POST
        {
            tmpCon = new System.Data.SqlClient.SqlConnection(saconstring);
            customer = CreateSlug(customer);
            String query = "INSERT INTO dbo.PARKINGS (CustomerSlug, Operater, EnterTime) VALUES ('[CustomerSlug]', [Operater], '[EnterTime]'; )";
            query = query.Replace("[CustomerSlug]", customer);
            query = query.Replace("[Operater]", operater);
            query = query.Replace("[EnterTime]", DateTime.Now.ToString("yyyyMMdd HH:mm:ss"));

            object res = GetNonQuery(tmpCon, query);
            if (Convert.ToInt32(res) > 0)
            {
                return "OK";
            }
            else
            {
                return "Greška prilikom unosa!";
            }
        }

        [Route("api/ParkingController/GetOpenedParkings/{customer}")]
        [System.Web.Http.AcceptVerbs("GET", "POST")]
        [System.Web.Http.HttpGet]
        public string GetOpenedParkings(String customer)  // POST
        {
            tmpCon = new System.Data.SqlClient.SqlConnection(saconstring);
            customer = CreateSlug(customer);
            DataTable tblOpenedParkigns = GetTable(tmpCon, "SELECT * FROM dbo.PARKINGS WHERE CustomerSlug = '" + customer + "' AND ExitTime IS NULL");

            if (tblOpenedParkigns.Rows.Count > 0)
            {
                return JsonConvert.SerializeObject(tblOpenedParkigns);
            }
            else
            {
                return "PRAZNO";
            }
        }


        [Route("api/ParkingController/UpdateOpenedParkings/{ID}")]
        [System.Web.Http.AcceptVerbs("GET", "POST")]
        [System.Web.Http.HttpGet]
        public string UpdateOpenedParkings(String ID)  // POST
        {
            CultureInfo MyCultureInfo = new CultureInfo("de-DE");
            tmpCon = new System.Data.SqlClient.SqlConnection(saconstring);
            String dtNow = DateTime.Now.ToString("yyyyMMdd HH:mm:ss");

            object EnterTime = GetScalar(tmpCon, "SELECT EnterTime FROM dbo.PARKINGS WHERE ID = " + ID);
            DateTime dtEnterTime = DateTime.Parse(Convert.ToString(EnterTime), MyCultureInfo);
            String prc = CalculateStays(dtEnterTime);

            object res = GetNonQuery(tmpCon, "UPDATE dbo.PARKINGS SET Price = " + prc.Replace(",",".") + ", ExitTime = '" + dtNow + "' WHERE ID = " + ID);
            if (Convert.ToInt32(res) > 0)
            {
                return "OK";
            }
            else
            {
                return "Greška prilikom unosa!";
            }
        }

        [Route("api/ParkingController/CalculateStay/{ID}")]
        [System.Web.Http.AcceptVerbs("GET", "POST")]
        [System.Web.Http.HttpGet]
        public string CalculateStay(String ID)  // POST
        {
            tmpCon = new System.Data.SqlClient.SqlConnection(saconstring);
            CultureInfo MyCultureInfo = new CultureInfo("de-DE");

            object EnterTime = GetScalar(tmpCon, "SELECT EnterTime FROM dbo.PARKINGS WHERE ID = " + ID);
            DateTime dtEnterTime = DateTime.Parse(Convert.ToString(EnterTime), MyCultureInfo);

            String prc = CalculateStays(dtEnterTime);
            return prc;

        }

        public string CalculateStays(DateTime dtEnterTime)
        {
            DateTime dtNow = DateTime.Now;
            
            double hours = (dtNow - dtEnterTime).TotalHours;
            if (hours > 4)
            {
                int intHours = Convert.ToInt16(hours) + 1;
                int diferenceThanFour = intHours - 4;

                double dblCalculatedPrice = 4 * price;
                double dblCalculateddiferenceThanFour = diferenceThanFour * priceMoreThanFour;


                double dblPrice = dblCalculateddiferenceThanFour + dblCalculatedPrice;
                return Convert.ToString(dblPrice);
            }
            else
            {
                int intHours = Convert.ToInt16(hours) + 1;
                double dblCalculatedPrice = intHours * price;
                return Convert.ToString(dblCalculatedPrice);
            }
        }

        [Route("api/ParkingController/TopOperater")]
        [System.Web.Http.AcceptVerbs("GET", "POST")]
        [System.Web.Http.HttpGet]
        public string TopOperater()  // POST
        {
            tmpCon = new System.Data.SqlClient.SqlConnection(saconstring);

            DataTable tblTopOperater = GetTable(tmpCon, "SELECT TOP 1 Operater, SUM(Price) AS Price FROM dbo.PARKINGS GROUP BY Operater ORDER BY Price DESC");
               
            if (tblTopOperater.Rows.Count > 0)
            {
                return JsonConvert.SerializeObject(tblTopOperater);
            }
            else
            {
                return "PRAZNO";
            }
        }

        [Route("api/ParkingController/TopCustomer")]
        [System.Web.Http.AcceptVerbs("GET", "POST")]
        [System.Web.Http.HttpGet]
        public string TopCustomer()  
        {
            tmpCon = new System.Data.SqlClient.SqlConnection(saconstring);

            DataTable tblTopCustomer = GetTable(tmpCon, "SELECT TOP 1 CustomerSlug, SUM(Price) AS Price FROM dbo.PARKINGS GROUP BY CustomerSlug ORDER BY Price DESC");

            if (tblTopCustomer.Rows.Count > 0)
            {
                return JsonConvert.SerializeObject(tblTopCustomer);
            }
            else
            {
                return "PRAZNO";
            }
        }


        public string CreateSlug(string Title)
        {
            string Slug = Title.ToLower();
            // Replace characters specific fo croatian language
            // You don't need this part for english language
            // Also, you can replace other characters specific for other languages
            // e.g. é to e for French language etc.
            Slug = Slug.Replace("č", "c");
            Slug = Slug.Replace("ć", "c");
            Slug = Slug.Replace("š", "s");
            Slug = Slug.Replace("ž", "z");
            Slug = Slug.Replace("đ", "dj");

            // Replace - with empty space
            Slug = Slug.Replace("-", " ");

            // Replace unwanted characters with space
            Slug = Regex.Replace(Slug, @"[^a-z0-9\s-]", " ");
            // Replace multple white spaces with single space
            Slug = Regex.Replace(Slug, @"\s+", " ").Trim();
            // Replace white space with -
            Slug = Slug.Replace(" ", "-");

            return Slug;
        }

        public static DataTable GetTable(SqlConnection Connection, string SQL, bool ShowError = false)
        {
            ConnectionState ps = Connection.State;

            DataTable tbl = new DataTable();
            SqlDataAdapter cmd = new SqlDataAdapter(SQL, Connection);
            cmd.SelectCommand.CommandTimeout = 0;
            try
            {
                Connection.Close();
                if (ps != ConnectionState.Open)
                    Connection.Open();
                cmd.Fill(tbl);
            }
            catch (Exception ex)
            {

                if (ShowError)
                    throw new Exception(ex.Message);
            }

            finally
            {
                cmd = null;
                if (ps == ConnectionState.Closed)
                    Connection.Close();
            }
            return tbl;
        }


        public static object GetScalar(SqlConnection Connection, string SQL, bool ShowError = false, bool KeepConnectionOpened = false)
        {
            ConnectionState ps = Connection.State;
            object dr = null;
            SqlCommand cmd = new SqlCommand(SQL, Connection);
            cmd.CommandTimeout = 0;

            if (ps != ConnectionState.Open)
                Connection.Open();

            try
            {
                dr = cmd.ExecuteScalar();
            }
            catch (Exception ex)
            {
             
            
            }
          
            finally
            {
                if (KeepConnectionOpened == true)
                {
                }
                else if (ps == ConnectionState.Closed)
                    Connection.Close();
            }
            cmd = null;
            return dr;
        }

        public static object GetNonQuery(SqlConnection Connection, string SQL, bool ShowError = false, bool LogException = false)
        {
            ConnectionState ps = Connection.State;

            object dr = null;
            SqlCommand cmd = new SqlCommand(SQL, Connection);
            cmd.CommandTimeout = 0;

            if (ps != ConnectionState.Open)
                Connection.Open();

            try
            {
                dr= cmd.ExecuteNonQuery();
                cmd = null;
            }
            catch (Exception ex)
            {
              
            }
         
            finally
            {
                if (ps == ConnectionState.Closed)
                    Connection.Close();
            }
            cmd = null;
            return dr;
        }





                //        CREATE TABLE[dbo].[PARKINGS]
                //        (
 
                //     [ID][int] IDENTITY(1,1) NOT NULL,

                //    [CustomerSlug] [nvarchar] (255) NULL,
                //	[Operater] [int] NULL,
                //	[EnterTime] [datetime] NULL,
                //	[ExitTime] [datetime] NULL,
                //	[Price] [decimal](18, 5) NULL,
                // CONSTRAINT[PK_PARKINGS] PRIMARY KEY CLUSTERED
                //(
                //   [ID] ASC
                //)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
                //) ON[PRIMARY]
                //GO




    }
}
