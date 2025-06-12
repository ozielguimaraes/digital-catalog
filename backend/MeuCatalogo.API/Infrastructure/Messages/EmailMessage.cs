namespace MeuCatalogo.API.Infrastructure.Messages;

public class EmailMessage
{
    public string Assunto { get; set; }
    public string Corpo { get; set; }
    public List<string> Destinatarios { get; private set; }
    public List<string> Attachments { get; private set; } = new();
    public List<string> ReplyTo { get; set; } = new();

    public EmailMessage(string assunto, string corpo, string destinatario)
        : this(assunto, corpo, new List<string> { destinatario })
    {
    }

    public EmailMessage(string assunto, string corpo, List<string> destinatarios)
    {
        Assunto = assunto;
        Corpo = corpo;
        Destinatarios = destinatarios;
    }

    public void AddAttachment(string filePath)
    {
        if (!string.IsNullOrEmpty(filePath))
            Attachments.Add(filePath);
    }

    public void AddAttachments(IEnumerable<string> filePaths)
    {
        Attachments.AddRange(filePaths);
    }
}
