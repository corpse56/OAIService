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
        foreach (string param in Request.Params)
        {
            string par=param.ToLower();
            if ((par != "listrecords") ||
                (par != "getrecord") ||
                (par != "identify") ||
                (par != "listidentifiers") ||
                (par != "resumptiontoken") ||
                (par != "from") ||
                (par != "until") ||
                (par != "metadataprefix"))
            {
                badArgument();
                return;
            }
        }


        from = new DateTime(1, 1, 1);
        until = new DateTime(1,1,1);
        XmlDocument doc=new XmlDocument();

        switch (verb)
        {
            case "listrecords":
                if ((Request["from"] == null) && (Request["until"] == null) && (Request["resumptiontoken"] == null))
                {
                    badArgument();
                    return;
                }
                if ((Request["from"] == null) && (Request["until"] == null) && (Request["resumptiontoken"] != null))
                {
                    //badArgument();
                    //здесь надо логику токена
                    return;
                }
                if ((Request["from"] != null) && (Request["until"] == null) && (Request["resumptiontoken"] == null))
                {
                    bool gooddate = DateTime.TryParse(Request["from"], out from);
                    if (!gooddate)
                    {
                        badArgument();
                        return;
                    }
                    until = DateTime.Today;
                }
                if ((Request["from"] != null) && (Request["until"] == null) && (Request["resumptiontoken"] != null))
                {
                    badArgument();
                }
                if ((Request["from"] == null) && (Request["until"] != null) && (Request["resumptiontoken"] == null))
                {
                    bool gooddate = DateTime.TryParse(Request["until"], out until);
                    if (!gooddate)
                    {
                        badArgument();
                        return;
                    }
                    from = new DateTime(2008, 12, 31);
                }
                if ((Request["from"] == null) && (Request["until"] != null) && (Request["resumptiontoken"] != null))
                {
                    badArgument();
                    return;
                }
                if ((Request["from"] != null) && (Request["until"] != null) && (Request["resumptiontoken"] == null))
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
                if ((Request["from"] != null) && (Request["until"] != null) && (Request["resumptiontoken"] != null))
                {
                    badArgument();
                    return;
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

                doc = ListRecords(true,"");//true - первый раз,false - по токену token
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

    private XmlDocument ListRecords(bool IsFirst,string ResumptionToken)
    {
        int IDRequest;
        DataTable Records;
        if (IsFirst)
        {
            IDRequest = InsertSessionPINS();
            Records = GetPinsByToken(IDRequest+"token1");
        }
        else
        {
            Records = GetPinsByToken(ResumptionToken);
            IDRequest = int.Parse(ResumptionToken.Substring(0,ResumptionToken.IndexOf("token")));;
        }

        int numtoken = int.Parse(ResumptionToken.Substring(ResumptionToken.IndexOf("token")+5));
        string lasttoken = GetLastToken(IDRequest);
        string currToken = Records.Rows[0]["TOKEN"].ToString();
        if (lasttoken == currToken)
        {
            ResumptionToken = "";//последний резумптионтокен
        }
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

        foreach (DataRow r in Records.Rows)
        {

            RMCONVERT rm = new RMCONVERT(r["BAZA"].ToString());
            rm.FormRUSM(Convert.ToInt32(r["IDMAIN"]));
            DS = new DataSet();
            DA = new SqlDataAdapter();
            DA.SelectCommand = new SqlCommand();
            DA.SelectCommand.Connection = new SqlConnection(XmlConnections.GetConnection("/Connections/base01"));
            DA.SelectCommand.CommandText = "select distinct MET,IND1,IND2,IDBLOCK from TECHNOLOG_VVV..RUSM where IDMAIN = " + r["IDMAIN"].ToString();
            int i = DA.Fill(DS, "rusm");

            XmlNode RecordNode = xmlDoc.CreateElement("record");
            ListRecords.AppendChild(RecordNode);
            XmlNode HeaderNode = xmlDoc.CreateElement("header");
            RecordNode.AppendChild(HeaderNode);

            node = xmlDoc.CreateElement("identifier");
            node.InnerText = "oai:aleph.nlr.ru:" + r["BAZA"].ToString() + r["IDMAIN"].ToString();
            HeaderNode.AppendChild(node);
            node = xmlDoc.CreateElement("datestamp");
            node.InnerText = Convert.ToDateTime(r["DATESTAMP"]).ToString("yyyy-MM-ddTHH:mm:ssZ");//"2015-03-05T02:04:40Z");
            HeaderNode.AppendChild(node);
            XmlNode MetaData = xmlDoc.CreateElement("metadata");
            RecordNode.AppendChild(MetaData);
            XmlNode MarcRecord = xmlDoc.CreateElement("marc", "record", "http://www.loc.gov/MARC21/slim");
            attribute = xmlDoc.CreateAttribute("xmlns:xsi");
            attribute.Value = "http://www.w3.org/2001/XMLSchema-instance";
            MarcRecord.Attributes.Append(attribute);
            attribute = xmlDoc.CreateAttribute("xsi:schemaLocation", "http://www.w3.org/2001/XMLSchema-instance");
            attribute.Value = "http://www.loc.gov/MARC21/slim http://www.loc.gov/standards/marcxml/schema/MARC21slim.xsd";
            MarcRecord.Attributes.Append(attribute);

            MetaData.AppendChild(MarcRecord);

            char c31 = (char)31;

            foreach (DataRow row in DS.Tables["rusm"].Rows)
            {
                if ((row["MET"].ToString() == "0") && (row["IND1"].ToString() == "0") && (row["IND2"].ToString() == "0"))
                {
                    DA.SelectCommand.CommandText = "select * from TECHNOLOG_VVV..RUSM where MET = " + row["MET"].ToString() + " and IND1 = '" + row["IND1"].ToString() + "' and IND2='" + row["IND2"].ToString() + "'";
                    if (DS.Tables["controlfield"] != null) { DS.Tables["controlfield"].Clear(); DS.Tables["controlfield"].AcceptChanges(); }//{ while (DS.Tables["controlfield"].Rows.Count > 0) DS.Tables["controlfield"].Rows.Remove(DS.Tables["controlfield"].Rows[0]); DS.Tables["controlfield"].AcceptChanges(); }
                    DA.Fill(DS, "controlfield");
                    node = xmlDoc.CreateElement("marc", "leader", "http://www.loc.gov/MARC21/slim");
                    node.InnerText = DS.Tables["controlfield"].Rows[0]["POL"].ToString();
                    MarcRecord.AppendChild(node);
                    continue;
                }

                if ((Convert.ToInt32(row["MET"]) < 10) && (Convert.ToInt32(row["MET"]) > 0))
                {
                    node = xmlDoc.CreateElement("marc", "controlfield", "http://www.loc.gov/MARC21/slim");
                    DA.SelectCommand.CommandText = "select * from TECHNOLOG_VVV..RUSM where MET = " + row["MET"].ToString() + " and IND1 = '" + row["IND1"].ToString() + "' and IND2='" + row["IND2"].ToString() + "'";
                    if (DS.Tables["controlfield"] != null) 
                    {
                        while (DS.Tables["controlfield"].Rows.Count > 0)
                        {
                            DS.Tables["controlfield"].Rows.RemoveAt(0);
                        }
                    }
                    DA.Fill(DS, "controlfield");
                    attribute = xmlDoc.CreateAttribute("tag");
                    string tag = row["MET"].ToString();
                    if (tag.Length == 1) tag = "00" + tag;
                    if (tag.Length == 2) tag = "0" + tag;
                    attribute.Value = tag;
                    node.Attributes.Append(attribute);
                    node.InnerText = DS.Tables["controlfield"].Rows[0]["POL"].ToString().Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
                    MarcRecord.AppendChild(node);
                    continue;
                }

                DA.SelectCommand.CommandText = "select * from TECHNOLOG_VVV..RUSM where MET = " + row["MET"].ToString() + " and IND1 = '" + row["IND1"].ToString() + "' and IND2='" + row["IND2"].ToString() + "' and IDBLOCK='" + row["IDBLOCK"].ToString() + "'";
                if (DS.Tables["subfield"] != null)
                {
                    while(DS.Tables["subfield"].Rows.Count>0)
                    {
                        DS.Tables["subfield"].Rows.RemoveAt(0);
                    }
                }

                int j = DA.Fill(DS, "subfield");

                node = xmlDoc.CreateElement("marc", "datafield", "http://www.loc.gov/MARC21/slim");
                attribute = xmlDoc.CreateAttribute("tag");
                attribute.Value = row["MET"].ToString();
                node.Attributes.Append(attribute);
                attribute = xmlDoc.CreateAttribute("ind1");
                attribute.Value = row["IND1"].ToString();
                node.Attributes.Append(attribute);
                attribute = xmlDoc.CreateAttribute("ind2");
                attribute.Value = row["IND2"].ToString();
                node.Attributes.Append(attribute);
                MarcRecord.AppendChild(node);


                XmlNode subf;
                foreach (DataRow rsub in DS.Tables["subfield"].Rows)
                {
                    string pol = rsub["POL"].ToString();
                    int k = pol.IndexOf(c31);

                    if (k > 0)
                    {
                        bool df;
                        if (r["IDMAIN"].ToString() == "3002")
                        {
                            df = true;
                        }
                        subf = xmlDoc.CreateElement("marc", "subfield", "http://www.loc.gov/MARC21/slim");
                        attribute = xmlDoc.CreateAttribute("code");
                        attribute.Value = rsub["IDENT"].ToString();
                        subf.Attributes.Append(attribute);
                        subf.InnerText = pol.Substring(0,k).Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
                        node.AppendChild(subf);

                        //pol = pol.Substring(k);

                        while (k > 0)
                        {
                            char ident = pol.Substring(k+1, 1)[0];
                            string ppol = pol.Substring(k+1);
                            if (ppol.IndexOf(c31) > 0)
                            {
                                ppol = ppol.Substring(1,ppol.IndexOf(c31) - 1);
                            }

                            pol = pol.Substring(k + 1);
                            k = pol.IndexOf(c31);
                            subf = xmlDoc.CreateElement("marc", "subfield", "http://www.loc.gov/MARC21/slim");
                            attribute = xmlDoc.CreateAttribute("code");
                            attribute.Value = ident.ToString();
                            subf.Attributes.Append(attribute);
                            if (k < 0)
                                ppol = ppol.Substring(1);
                            subf.InnerText = ppol.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
                            node.AppendChild(subf);

                        }
                    }
                    else
                    {
                        subf = xmlDoc.CreateElement("marc", "subfield", "http://www.loc.gov/MARC21/slim");
                        attribute = xmlDoc.CreateAttribute("code");
                        attribute.Value = rsub["IDENT"].ToString();
                        subf.Attributes.Append(attribute);
                        subf.InnerText = pol.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
                        node.AppendChild(subf);
                    }
                    //ident = pol.Substring(0, 1)[0];
                    //ppol = pol.Substring(1);
                    //fsout_str(fsout, "<marc:subfield code=\"" + ident + "\">" + ppol.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;") + "</marc:subfield>");
                }

            }//foreach RUSM
            //$d /mnt/fs-share/BJVVV/1/319/837 – абсолютный путь до папки с файлом
            //$f book.pdf – имя файла
            //$y file – тип доступа
            int PdfExists=0;
            if (rm.BAZA == "BJVVV")
            {
                DA = new SqlDataAdapter();
                DA.SelectCommand = new SqlCommand();
                DA.SelectCommand.Connection = new SqlConnection(XmlConnections.GetConnection("/Connections/base01"));
                DA.SelectCommand.CommandText = "select * from [BookAddInf].[dbo].[ScanInfo] where IDBook = " + r["IDMAIN"].ToString() + 
                                               " and IDBASE = 1 and PDF = 1"; ;
                PdfExists = DA.Fill(DS, "pdf");
            }
            else
            {
                DA = new SqlDataAdapter();
                DA.SelectCommand = new SqlCommand();
                DA.SelectCommand.Connection = new SqlConnection(XmlConnections.GetConnection("/Connections/base01"));
                DA.SelectCommand.CommandText = "select * from [BookAddInf].[dbo].[ScanInfo] where IDBook = " + r["IDMAIN"].ToString() + 
                                               " and IDBASE = 2 and PDF = 1"; 
                PdfExists = DA.Fill(DS, "pdf");
            }
            if (PdfExists == 0)
            {
                continue;
            }
            string path = DS.Tables["PDF"].Rows[0]["IDBook"].ToString();
            switch (path.Length)
            {
                case 1:
                    path = "000000" + path;
                    break;
                case 2:
                    path = "00000" + path;
                    break;
                case 3:
                    path = "0000" + path;
                    break;
                case 4:
                    path = "000" + path;
                    break;
                case 5:
                    path = "00" + path;
                    break;
                case 6:
                    path = "0" + path;
                    break;
            }
            if (rm.BAZA == "BJVVV")
            {
                path = "/mnt/fs-share/BJVVV/" + path[0] + @"/" + path[1] + path[2] + path[3] + @"/" + path[4] + path[5] + path[6];
            }
            else
            {
                path = "/mnt/fs-share/REDKOSTJ/" + path[0] + @"/" + path[1] + path[2] + path[3] + @"/" + path[4] + path[5] + path[6];
            }
            node = xmlDoc.CreateElement("marc", "datafield", "http://www.loc.gov/MARC21/slim");
            attribute = xmlDoc.CreateAttribute("tag");
            attribute.Value = "856";
            node.Attributes.Append(attribute);
            attribute = xmlDoc.CreateAttribute("ind1");
            attribute.Value = "7";
            node.Attributes.Append(attribute);
            attribute = xmlDoc.CreateAttribute("ind2");
            attribute.Value = "2";
            node.Attributes.Append(attribute);
            MarcRecord.AppendChild(node);


            
            
            XmlNode subpdf = xmlDoc.CreateElement("marc", "subfield", "http://www.loc.gov/MARC21/slim");
            attribute = xmlDoc.CreateAttribute("code");
            attribute.Value = "d";
            subpdf.Attributes.Append(attribute);
            subpdf.InnerText = path;
            node.AppendChild(subpdf);
            subpdf = xmlDoc.CreateElement("marc", "subfield", "http://www.loc.gov/MARC21/slim");
            attribute = xmlDoc.CreateAttribute("code");
            attribute.Value = "f";
            subpdf.Attributes.Append(attribute);
            subpdf.InnerText = "book.pdf";
            node.AppendChild(subpdf);
            subpdf = xmlDoc.CreateElement("marc", "subfield", "http://www.loc.gov/MARC21/slim");
            attribute = xmlDoc.CreateAttribute("code");
            attribute.Value = "y";
            subpdf.Attributes.Append(attribute);
            subpdf.InnerText = "file";
            node.AppendChild(subpdf);


        }

        node = xmlDoc.CreateElement("resumptionToken");
        node.InnerText = ;
        ListRecords.AppendChild(node);



        //<request verb="ListRecords" from="2015-03-05" until="2015-03-05" metadataPrefix="marc21">http://aleph.nlr.ru/OAI</request>
        //<request metadataPrefix="marc21" from="2015-04-14" until="2015-05-31" verb="ListRecords">http://aleph.nlr.ru/OAI</request>

        return xmlDoc;
    }

    private string GetLastToken(int IDRequest)
    {
        DS = new DataSet();
        DA = new SqlDataAdapter();
        DA.SelectCommand = new SqlCommand();
        DA.SelectCommand.Connection = new SqlConnection(XmlConnections.GetConnection("/Connections/base03"));
        DA.SelectCommand.CommandText = "select distinct TOKEN from EXPORTNEB..PINSFORREQUEST where IDREQUEST = " + IDRequest;
        int i = DA.Fill(DS, "pinsforrequest");
        string token = DS.Tables["pinsforrequest"].Rows[0]["TOKEN"].ToString();
        int max = int.Parse(token.Substring(0, token.IndexOf("token")));

        foreach (DataRow r in DS.Tables["pinsforrequest"].Rows)
        {
            string nexttoken = r["TOKEN"].ToString();
            int next = int.Parse(nexttoken.Substring(0, nexttoken.IndexOf("token")));

            if (max < next)
            {
                max = next;
            }
        }
        return IDRequest.ToString()+"token"+max.ToString();
    }

    private DataTable GetPinsByToken(string token)
    {
        DS = new DataSet();
        DA = new SqlDataAdapter();
        DA.SelectCommand = new SqlCommand();
        DA.SelectCommand.Connection = new SqlConnection(XmlConnections.GetConnection("/Connections/base03"));
        DA.SelectCommand.CommandText = "select * from EXPORTNEB..PINSFORREQUEST where TOKEN = '"+token+"'";
        int i = DA.Fill(DS, "pinsforrequest");
        return DS.Tables["pinsforrequest"];
    }

    private int InsertSessionPINS()
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
            DA.InsertCommand.CommandText = "insert into EXPORTNEB..REQUESTSHISTORY (SESSIONID,HOSTIP,VERB,FROMDATE,UNTILDATE,REQUESTDATE,FIRSTTOKEN,TOKENEXPIRE,COMPLETELISTSIZE) " +
                                           " select SESSIONID,HOSTIP,VERB,FROMDATE,UNTILDATE,REQUESTDATE,FIRSTTOKEN,TOKENEXPIRE,COMPLETELISTSIZE from EXPORTNEB..CURRENTREQUESTS where ID = " + DS.Tables["sess"].Rows[0]["ID"].ToString();
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
        DA.InsertCommand.CommandText =
            " with A as ( "+
        //SELECT IDMAIN,'BJVVV' baza  FROM BJVVV..DATAEXT A where IDMAIN in (1365206,1365214,1365215,1365225,1365351,1365357,1365372) union all " +
           "  SELECT DISTINCT IDMAIN,'BJVVV' baza,B.DateChange datestamp  FROM BJVVV..DATAEXT A" +
           "     LEFT join BJVVV..MAIN B on A.IDMAIN = B.ID " +
           "     WHERE SORT = 'Длявыдачи' AND MNFIELD=921 AND MSFIELD='$c'  " +
           "     AND IDMAIN NOT IN  " +
           "    (SELECT IDMAIN FROM  BJVVV..DATAEXT " +
           "     WHERE SORT = 'Учетнаязапись' AND MNFIELD=899 AND MSFIELD='$x')       " +
           "  union all " +
           "  SELECT DISTINCT IDMAIN ,'REDKOSTJ' baza,C.DateChange datestamp FROM REDKOSTJ..DATAEXT A" +
           "     LEFT join REDKOSTJ..MAIN C on A.IDMAIN = C.ID " +
           "     WHERE SORT = 'Длявыдачи' AND MNFIELD=921 AND MSFIELD='$c'  " +
           "     AND IDMAIN NOT IN  " +
           "    (SELECT IDMAIN FROM  REDKOSTJ..DATAEXT " +
           "     WHERE SORT = 'Учетнаязапись' AND MNFIELD=899 AND MSFIELD='$x') " +
           "  ), " +
           "  B as ( " +
           "  select row_number() over (order by A.IDMAIN) num ,A.IDMAIN,baza, " +
           "     datestamp " +
           "     from A " +
           "  ) " +
           "  insert into EXPORTNEB..PINSFORREQUEST ([CURSOR],IDREQUEST, IDMAIN,BAZA,DATESTAMP,TOKEN,NEXTTOKEN) " +
           "  select num," + IDRequest + ",IDMAIN,baza,datestamp, '" + IDRequest + "'+'token' + cast(((row_number() over(order by num) - 1) / 30) + 1 as nvarchar(100)) as TOKEN, " +
           " '"+IDRequest+"'+'token' + cast(((row_number() over(order by num) - 1) / 30) + 2 as nvarchar(100)) as NEXTTOKEN " +
           "  from B where CAST(CAST(datestamp AS date) AS datetime) between '" + from.ToString("yyyyMMdd") + "' and '" + until.ToString("yyyyMMdd") + "' order by datestamp";
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
        return IDRequest;


    }
}
