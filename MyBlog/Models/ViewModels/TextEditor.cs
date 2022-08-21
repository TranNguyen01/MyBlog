namespace MyBlog.Models.ViewModels
{
    public class TextEditor
    {
        public TextEditor(string iD, string placeholder, bool importScript = true, int tabSize = 2, int height = 500)
        {
            ID = iD;
            ImportScript = importScript;
            Placeholder = placeholder;
            TabSize = tabSize;
            Height = height;
        }

        public string ID { get; set; }

        public bool ImportScript { get; set; }

        public string Placeholder { get; set; }

        public int TabSize { get; set; }

        public int Height { get; set; }

        public string Toolbar { get; set; } = @"[
        ['style', ['style']],
        ['font', ['bold', 'underline', 'clear']],
        ['color', ['color']],
        ['para', ['ul', 'ol', 'paragraph']],
        ['table', ['table']],
        ['insert', ['link', 'picture', 'video']],
        ['view', ['fullscreen', 'codeview', 'help']]
    ]";
    }
}
