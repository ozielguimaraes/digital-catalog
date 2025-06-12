using System.Net;
using System.Net.Mail;
using System.Text;

namespace MeuCatalogo.API.Infrastructure.Messages;

public class EmailSender
{
    private readonly string _remetente;
    private readonly string _senha;
    private readonly string _host;
    private readonly int _porta;
    private readonly bool _enableSsl;

    public EmailSender(string remetente, string senha, string host, int porta, bool enableSsl = true)
    {
        _remetente = remetente;
        _senha = senha;
        _host = host;
        _porta = porta;
        _enableSsl = enableSsl;
    }

    public async Task<bool> EnviarEmailAsync(EmailMessage message, string displayName = "Aplicativo")
    {
        try
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(_remetente, displayName),
                Priority = MailPriority.Normal,
                IsBodyHtml = true,
                Subject = message.Assunto,
                Body = message.Corpo,
                SubjectEncoding = Encoding.UTF8,
                BodyEncoding = Encoding.UTF8
            };

            foreach (var destinatario in message.Destinatarios)
                mailMessage.To.Add(destinatario);

            foreach (var replyTo in message.ReplyTo)
                mailMessage.ReplyToList.Add(new MailAddress(replyTo));

            foreach (var attachment in message.Attachments)
                mailMessage.Attachments.Add(new Attachment(attachment));

            using var smtp = new SmtpClient
            {
                Host = _host,
                Port = _porta,
                EnableSsl = _enableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(_remetente, _senha)
            };

            await smtp.SendMailAsync(mailMessage);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
