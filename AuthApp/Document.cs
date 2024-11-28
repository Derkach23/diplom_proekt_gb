public class Document
{
    public int Id { get; set; }
    public string DocumentName { get; set; }
    public byte[] Content { get; set; }
    public int UploadedBy { get; set; }
    public DateTime UploadDate { get; set; }
}