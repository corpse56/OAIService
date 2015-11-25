using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Data.SqlClient;
using System.Data;
using System.IO;

public partial class _Default : System.Web.UI.Page 
{
    SqlDataAdapter DA,DAT;
    DataSet DS;
    DateTime from, until;
    string BAZA;
    protected void Page_Load(object sender, EventArgs e)
    {
        //?verb=ListRecords&from=2015-03-05&metadataPrefix=marc21&until=2015-03-05
        string verb="";
        if (Request["verb"] != null)
        {
            verb = Request["verb"].ToLower();
        }
        if ((verb != "listrecords") && (verb != "getrecord") && (verb != "identify") && (verb != "listidentifiers"))
        {
            badVerb();
            return;
        }
        from = new DateTime(1, 1, 1);
        until = new DateTime(1,1,1);
        XmlDocument doc=new XmlDocument();

        switch (Request["verb"])
        {
            case "listrecords":
                if ((Request["from"] == null) && (Request["until"] == null))
                {
                    badArgument();
                    return;
                }
                if ((Request["from"] != null) && (Request["until"] == null))
                {
                    bool gooddate = DateTime.TryParse(Request["from"], out from);
                    if (!gooddate)
                    {
                        badArgument();
                        return;
                    }
                    until = DateTime.Today;
                }
                if ((Request["from"] == null) && (Request["until"] != null))
                {
                    bool gooddate = DateTime.TryParse(Request["until"], out until);
                    if (!gooddate)
                    {
                        badArgument();
                        return;
                    }
                    from = new DateTime(2008, 12, 31);
                }
                if ((Request["from"] != null) && (Request["until"] != null))
                {
                    bool gooddate = DateTime.TryParse(Request["from"], out from);
                    if (!gooddate)
                    {
                        badArgument();
                        return;
                    }
                    gooddate = DateTime.TryParse(Request["until"], out until);
                    if (!gooddate)
                    {
                        badArgument();
                        return;
                    }
                }
                if (from > until)
                {
                    badArgument();
                    return;
                }

                if (Request["metadataPrefix"] == null)
                {
                    badArgument();
                    return;
                }
                if (Request["metadataPrefix"] != "marc21")
                {
                    badArgument();
                }

                doc = ListRecords();
                break;
            default:

                //return x;
                break;
        }


        Response.Clear(); //Optional: if we've sent anything before
        Response.ContentType = "text/xml"; //Must be 'text/xml'
        Response.ContentEncoding = System.Text.Encoding.UTF8; //We'd like UTF-8
        doc.Save(Response.Output); //Save to the text-writer
        ////using the encoding of the text-writer
        ////(which comes from response.contentEncoding)
        Response.End(); //Optional: will end processing
    }



    private void badArgument()
    {
            //<?xml version="1.0" encoding="UTF-8"?>
            //<OAI-PMH xmlns="http://www.openarchives.org/OAI/2.0/" 
            //         xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
            //         xsi:schemaLocation="http://www.openarchives.org/OAI/2.0/
            //         http://www.openarchives.org/OAI/2.0/OAI-PMH.xsd">
            //  <responseDate>2002-06-01T19:20:30Z</responseDate> 
            //  <request verb="ListRecords" from="2002-06-01T02:00:00Z"
            //           until="2002-06-01T03:020:00Z"
            //           metadataPrefix="oai_marc">
            //           http://memory.loc.gov/cgi-bin/oai</request>
            //  <error code="badArgument"/>
            //</OAI-PMH>    
        XmlDocument xmlDoc = new XmlDocument();
        XmlNode rootNode = xmlDoc.CreateElement("OAI-PMH");
        XmlAttribute attribute = xmlDoc.CreateAttribute("xmlns");
        attribute.Value = "http://www.openarchives.org/OAI/2.0/";
        rootNode.Attributes.Append(attribute);
        attribute = xmlDoc.CreateAttribute("xmlns:xsi");
        attribute.Value = "http://www.w3.org/2001/XMLSchema-instance";
        rootNode.Attributes.Append(attribute);
        attribute = xmlDoc.CreateAttribute("xsi:schemaLocation", "http://www.w3.org/2001/XMLSchema-instance");
        attribute.Value = "http://www.openarchives.org/OAI/2.0/ http://www.openarchives.org/OAI/2.0/OAI-PMH.xsd";
        rootNode.Attributes.Append(attribute);
        xmlDoc.AppendChild(rootNode);
        XmlNode node = xmlDoc.CreateElement("responseDate");
        node.InnerText = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ");
        rootNode.AppendChild(node);
        node = xmlDoc.CreateElement("request");
        node.InnerText = Request.Url.AbsoluteUri;
        rootNode.AppendChild(node);
        node = xmlDoc.CreateElement("error");
        attribute = xmlDoc.CreateAttribute("code");
        attribute.Value = "badArgument";
        node.Attributes.Append(attribute);
        rootNode.AppendChild(node);

        Response.Clear();
        Response.ContentType = "text/xml";
        Response.ContentEncoding = System.Text.Encoding.UTF8;
        xmlDoc.Save(Response.Output);
        Response.End(); 
    }

    private void badVerb()
    {
        //<?xml version="1.0" encoding="UTF-8"?>
        //<OAI-PMH xmlns="http://www.openarchives.org/OAI/2.0/" 
        //         xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
        //         xsi:schemaLocation="http://www.openarchives.org/OAI/2.0/
        //         http://www.openarchives.org/OAI/2.0/OAI-PMH.xsd">
        //  <responseDate>2002-05-01T09:18:29Z</responseDate>
        //  <request>http://arXiv.org/oai2</request>
        //  <error code="badVerb">Illegal OAI verb</error>
        //</OAI-PMH>
        
        XmlDocument xmlDoc = new XmlDocument();
        XmlNode rootNode = xmlDoc.CreateElement("OAI-PMH");
        XmlAttribute attribute = xmlDoc.CreateAttribute("xmlns");
        attribute.Value = "http://www.openarchives.org/OAI/2.0/";
        rootNode.Attributes.Append(attribute);
        attribute = xmlDoc.CreateAttribute("xmlns:xsi");
        attribute.Value = "http://www.w3.org/2001/XMLSchema-instance";
        rootNode.Attributes.Append(attribute);
        attribute = xmlDoc.CreateAttribute("xsi:schemaLocation", "http://www.w3.org/2001/XMLSchema-instance");
        attribute.Value = "http://www.openarchives.org/OAI/2.0/ http://www.openarchives.org/OAI/2.0/OAI-PMH.xsd";
        rootNode.Attributes.Append(attribute);
        xmlDoc.AppendChild(rootNode);
        XmlNode node = xmlDoc.CreateElement("responseDate");
        node.InnerText = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ");
        rootNode.AppendChild(node);
        node = xmlDoc.CreateElement("request");
        node.InnerText = Request.Url.AbsoluteUri;
        rootNode.AppendChild(node);
        node = xmlDoc.CreateElement("error");
        attribute = xmlDoc.CreateAttribute("code");
        attribute.Value = "badVerb";
        node.InnerText = "Illegal OAI verb";
        rootNode.AppendChild(node);

        Response.Clear(); 
        Response.ContentType = "text/xml"; 
        Response.ContentEncoding = System.Text.Encoding.UTF8;
        xmlDoc.Save(Response.Output); 
        Response.End(); 
        //return xmlDoc;

    }

    private XmlDocument ListRecords()
    {
        InsertSessionPINS();


        XmlDocument xmlDoc = new XmlDocument();
        XmlNode rootNode = xmlDoc.CreateElement("OAI-PMH");
        XmlAttribute attribute = xmlDoc.CreateAttribute("xmlns");
        attribute.Value = "http://www.openarchives.org/OAI/2.0/";
        rootNode.Attributes.Append(attribute);
        attribute = xmlDoc.CreateAttribute("xmlns:xsi");
        attribute.Value = "http://www.w3.org/2001/XMLSchema-instance";
        rootNode.Attributes.Append(attribute);
        attribute = xmlDoc.CreateAttribute("xsi:schemaLocation", "http://www.w3.org/2001/XMLSchema-instance");
        attribute.Value = "http://www.openarchives.org/OAI/2.0/ http://www.openarchives.org/OAI/2.0/OAI-PMH.xsd";
        rootNode.Attributes.Append(attribute);
        xmlDoc.AppendChild(rootNode);
        XmlNode node = xmlDoc.CreateElement("responseDate");
        node.InnerText = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ");
        rootNode.AppendChild(node);

        node = xmlDoc.CreateElement("request");
        attribute = xmlDoc.CreateAttribute("verb");
        attribute.Value = Request["verb"];
        node.Attributes.Append(attribute);
        attribute = xmlDoc.CreateAttribute("from");
        attribute.Value = Request["from"];
        node.Attributes.Append(attribute);
        attribute = xmlDoc.CreateAttribute("until");
        attribute.Value = Request["until"];
        node.Attributes.Append(attribute);
        attribute = xmlDoc.CreateAttribute("metadataPrefix");
        attribute.Value = Request["metadataPrefix"];
        node.Attributes.Append(attribute);
        rootNode.AppendChild(node);

        XmlNode ListRecords = xmlDoc.CreateElement("ListRecords");
        rootNode.AppendChild(ListRecords);
        node = xmlDoc.CreateElement("record");
        ListRecords.AppendChild(node);
        node = xmlDoc.CreateElement("header");




        //<request verb="ListRecords" from="2015-03-05" until="2015-03-05" metadataPrefix="marc21">http://aleph.nlr.ru/OAI</request>
        //<request metadataPrefix="marc21" from="2015-04-14" until="2015-05-31" verb="ListRecords">http://aleph.nlr.ru/OAI</request>

        return xmlDoc;
    }

    private void InsertSessionPINS()
    {
        DS = new DataSet();
        DA = new SqlDataAdapter();
        DA.SelectCommand = new SqlCommand();
        DA.SelectCommand.Connection = new SqlConnection(XmlConnections.GetConnection("/Connections/base03"));
        DA.SelectCommand.Parameters.Add("SESSIONID", SqlDbType.NVarChar);
        DA.SelectCommand.Parameters.Add("HOSTIP", SqlDbType.NVarChar);
        DA.SelectCommand.Parameters.Add("VERB", SqlDbType.NVarChar);
        DA.SelectCommand.Parameters["SESSIONID"].Value = Session.SessionID;
        DA.SelectCommand.Parameters["HOSTIP"].Value = Request.UserHostAddress;
        DA.SelectCommand.Parameters["VERB"].Value = Request["verb"];

        DA.SelectCommand.CommandText = "select * from EXPORTNEB..CURRENTREQUESTS where verb = @VERB and SESSIONID = @SESSIONID and HOSTIP = @HOSTIP";
        int i = DA.Fill(DS, "sess");
        DA.SelectCommand.Parameters.Clear();
        if (i != 0)
        {
            DA.InsertCommand = new SqlCommand();
            DA.InsertCommand.Connection = new SqlConnection(XmlConnections.GetConnection("/Connections/base03"));
            DA.InsertCommand.CommandText = "insert into EXPORTNEB..REQUESTSHISTORY (SESSIONID,HOSTIP,VERB,FROMDATE,UNTILDATE,REQUESTDATE) "+
                                           " select SESSIONID,HOSTIP,VERB,FROMDATE,UNTILDATE,REQUESTDATE from EXPORTNEB..CURRENTREQUESTS where ID = "+DS.Tables["sess"].Rows[0]["ID"].ToString();
            DA.InsertCommand.Connection.Open();
            DA.InsertCommand.ExecuteNonQuery();
            DA.InsertCommand.Connection.Close();

            DA.DeleteCommand = new SqlCommand();
            DA.DeleteCommand.Connection = new SqlConnection(XmlConnections.GetConnection("/Connections/base03"));
            DA.DeleteCommand.CommandText = "delete from EXPORTNEB..CURRENTREQUESTS where ID = "+DS.Tables["sess"].Rows[0]["ID"].ToString()+"; "+
                                           "delete from EXPORTNEB..PINSFORREQUEST where IDREQUEST = "+DS.Tables["sess"].Rows[0]["ID"].ToString() ;
            DA.DeleteCommand.Connection.Open();
            DA.DeleteCommand.ExecuteNonQuery();
            DA.DeleteCommand.Connection.Close();

        }
        DA.InsertCommand = new SqlCommand();
        DA.InsertCommand.Connection = new SqlConnection(XmlConnections.GetConnection("/Connections/base03"));
        DA.InsertCommand.Parameters.Clear();
        DA.InsertCommand.Parameters.Add("SESSIONID", SqlDbType.NVarChar);
        DA.InsertCommand.Parameters.Add("HOSTIP", SqlDbType.NVarChar);
        DA.InsertCommand.Parameters.Add("VERB", SqlDbType.NVarChar);
        DA.InsertCommand.Parameters.Add("FROMDATE", SqlDbType.DateTime);
        DA.InsertCommand.Parameters.Add("UNTILDATE", SqlDbType.DateTime);
        DA.InsertCommand.Parameters.Add("REQUESTDATE", SqlDbType.DateTime);
        DA.InsertCommand.Parameters.Add("FIRSTTOKEN", SqlDbType.NVarChar);
        DA.InsertCommand.Parameters.Add("TOKENEXPIRE", SqlDbType.DateTime);
        DA.InsertCommand.Parameters.Add("COMPLETELISTSIZE", SqlDbType.Int);

        DA.InsertCommand.Parameters["SESSIONID"].Value = Session.SessionID;
        DA.InsertCommand.Parameters["HOSTIP"].Value = Request.UserHostAddress;
        DA.InsertCommand.Parameters["VERB"].Value = Request["verb"];
        DA.InsertCommand.Parameters["FROMDATE"].Value = from;
        DA.InsertCommand.Parameters["UNTILDATE"].Value = until;
        DA.InsertCommand.Parameters["REQUESTDATE"].Value = DateTime.Now;
        DA.InsertCommand.Parameters["FIRSTTOKEN"].Value = "1";
        DA.InsertCommand.Parameters["TOKENEXPIRE"].Value = DateTime.Now.AddDays(2);
        DA.InsertCommand.Parameters["COMPLETELISTSIZE"].Value = -1;

        DA.InsertCommand.CommandText = "insert into EXPORTNEB..CURRENTREQUESTS (SESSIONID,HOSTIP,VERB,FROMDATE,UNTILDATE,REQUESTDATE,FIRSTTOKEN,TOKENEXPIRE,COMPLETELISTSIZE) " +
                                       " values (@SESSIONID,@HOSTIP,@VERB,@FROMDATE,@UNTILDATE,@REQUESTDATE,@FIRSTTOKEN,@TOKENEXPIRE,@COMPLETELISTSIZE); select scope_identity()";
        DA.InsertCommand.Connection.Open();
        object o = DA.InsertCommand.ExecuteScalar();
        int IDRequest = Convert.ToInt32(o);
        DA.InsertCommand.Connection.Close();



        DA.InsertCommand.Parameters.Clear();
        DA.InsertCommand.CommandText = " with A as ( " +
                                       "  SELECT DISTINCT IDMAIN,'BJVVV' baza  FROM BJVVV..DATAEXT " +
                                       " WHERE SORT = 'Длявыдачи' AND MNFIELD=921 AND MSFIELD='$c'  " +
                                       "  AND IDMAIN NOT IN  " +
                                       "  (SELECT IDMAIN FROM  BJVVV..DATAEXT " +
                                       "     WHERE SORT = 'Учетнаязапись' AND MNFIELD=899 AND MSFIELD='$x')      " +
                                       "  union all " +
                                       "  SELECT DISTINCT IDMAIN ,'REDKOSTJ' baza FROM REDKOSTJ..DATAEXT " +
                                       "  WHERE SORT = 'Длявыдачи' AND MNFIELD=921 AND MSFIELD='$c'  " +
                                       "  AND IDMAIN NOT IN  " +
                                       "  (SELECT IDMAIN FROM  REDKOSTJ..DATAEXT " +
                                       "     WHERE SORT = 'Учетнаязапись' AND MNFIELD=899 AND MSFIELD='$x') " +
                                       "     ), " +
                                       "     B as ( " +
                                       "     select row_number() over (order by A.IDMAIN) num ,A.IDMAIN,baza, " +
                                       "     EXPORTNEB.dbo.GetOAIDatestamp(A.IDMAIN,baza) datestamp " +
                                       "     from A " +
                                       "     ) " +
                                       "  insert into EXPORTNEB..PINSFORREQUEST ([CURSOR],IDREQUEST, IDMAIN,BAZA,DATESTAMP,TOKEN,NEXTTOKEN) " +
                                       " select num,"+IDRequest+",IDMAIN,baza,datestamp, '2'+'token' + cast(((row_number() over(order by num) - 1) / 30) + 1 as nvarchar(100)) as TOKEN, " +
                                       " '"+IDRequest+"'+'token' + cast(((row_number() over(order by num) - 1) / 30) + 2 as nvarchar(100)) as NEXTTOKEN " +
                                       " from B ";
        //DA.Fill(DS, "pins");
        DA.InsertCommand.CommandTimeout = 1200;
        DA.InsertCommand.Connection.Open();
        int ListSize = DA.InsertCommand.ExecuteNonQuery();
        DA.InsertCommand.Connection.Close();


        DA.UpdateCommand = new SqlCommand();
        DA.UpdateCommand.Connection = new SqlConnection(XmlConnections.GetConnection("/Connections/base03"));
        DA.UpdateCommand.CommandText = "update EXPORTNEB..CURRENTREQUESTS set FIRSTTOKEN = '" + IDRequest.ToString() + "token1" + "',COMPLETELISTSIZE = "+ListSize+" where ID = " + IDRequest;
        DA.UpdateCommand.Connection.Open();
        DA.UpdateCommand.ExecuteNonQuery();
        DA.UpdateCommand.Connection.Close();


    }
    public class XmlConnections
    {

        private static String filename = System.AppDomain.CurrentDomain.BaseDirectory + "DBConnections.xml";
        private static XmlDocument doc;

        public static string GetConnection(string s)
        {
            if (!File.Exists(filename))
            {
                throw new Exception("Файл с подключениями 'DBConnections.xml' не найден.");
            }
            try
            {
                doc = new XmlDocument();
                doc.Load(filename);
            }
            catch
            {
                //MessageBox.Show(ex.Message);
                throw;
            }
            XmlNode node;
            try
            {
                node = doc.SelectSingleNode(s);
            }
            catch
            {
                throw new Exception("Узел " + s + " не найден в файле DBConnections.xml"); ;
            }

            return node.InnerText;
        }
        public XmlConnections()
        {

        }
    }
}
