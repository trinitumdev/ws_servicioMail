using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
public class Service : System.Web.Services.WebService
{
    public string cdgEmpresa;
    public string usuarioMail;
    public string passMail;
    public string smtpMail;
    public string puertoMail;

    public Service()
    {
        cdgEmpresa = ConfigurationManager.AppSettings.Get("Empresa");
        usuarioMail = ConfigurationManager.AppSettings.Get("usuarioMail");
        passMail = ConfigurationManager.AppSettings.Get("passMail");
        smtpMail = ConfigurationManager.AppSettings.Get("smtpMail");
        puertoMail = ConfigurationManager.AppSettings.Get("puertoMail");
    }

    //METODO QUE PERMITE ENVIAR CORREO ELECTRONICO SOBRE CUALQUIER ASUNTO
    [WebMethod]
    public string setEnviaEmail(string emisor, string receptor, string asunto, string contenido)
    {
        if (emisor == "")
            emisor = usuarioMail;

        //receptor = "subgerente.sistemas@oportunidadtupatrimonio.com";
        string FROM = emisor;
        //string FROM = "subgerente.sistemas@oportunidadtupatrimonio.com";
        string FROMNAME = "Notificaciones OportunidadTuPatrimonio";
        string SMTP_USERNAME = usuarioMail;
        string SMTP_PASSWORD = passMail;
        string HOST = smtpMail;
        int PORT = int.Parse(puertoMail);
        string SUBJECT = asunto;

        Console.WriteLine(contenido);
        contenido = HttpUtility.HtmlDecode(contenido);

        string BODY = string.Format(@"<!DOCTYPE html>
                                    <html lang='es'>
                                      <head>
                                        <meta charset='utf-8'>
                                        <title>OportunidadTuPatrimonio</title>
                                      </head>
                                      <body>
                                        <table style='max-width: 800px; padding: 10px; margin:0 auto; border-collapse: collapse;'>
                                          <tr>
                                            <td style='padding: 0;'>
                                                <img alt='Ven-SumaT' style='padding: 0; display:block;' src='cid:encabezado' width='30%' height='30%'>
                                            </td>
                                          </tr>"
                                          +contenido+
                                        @"</table>
                                      </body>
                                    </html>");

        MailMessage message = new MailMessage();

        AlternateView view = AlternateView.CreateAlternateViewFromString(BODY, Encoding.UTF8, MediaTypeNames.Text.Html);

        string mediaType = MediaTypeNames.Image.Jpeg;
        string path = AppDomain.CurrentDomain.BaseDirectory;

        LinkedResource resource = new LinkedResource(@"" + path + "img/encabezado.png", mediaType);
        resource.ContentId = "encabezado";
        resource.ContentType.MediaType = mediaType;
        resource.TransferEncoding = TransferEncoding.Base64;
        resource.ContentType.Name = resource.ContentId;
        resource.ContentLink = new Uri("cid:" + resource.ContentId);
        view.LinkedResources.Add(resource);

        message.AlternateViews.Add(view);

        message.IsBodyHtml = true;
        message.From = new MailAddress(FROM, FROMNAME);

        string[] arrRecep = receptor.Split(',');

        for (int i = 0; i < arrRecep.Length; i++)
            message.To.Add(new MailAddress(arrRecep[i].Trim()));

        message.Subject = SUBJECT;
        message.Body = BODY;

        using (var client = new SmtpClient(HOST, PORT))
        {
            client.Credentials = new NetworkCredential(SMTP_USERNAME, SMTP_PASSWORD);
            client.EnableSsl = true;
            
            try
            {
                Console.WriteLine("Attempting to send email...");
                client.Send(message);
                Console.WriteLine("Email sent!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("The email was not sent.");
                Console.WriteLine("Error message: " + ex.Message);
                return "0 " + ex.Message;
            }
        }
        return "1";
    }
}
