using System.Text;

namespace EestiQuizer.Ekilex;

class HtmlImageListGenerator
{
    //>> example
    //
    //<!-- Example Gallery HTML -->
    //<html>
    //<head>
    //    <style>
    //        .gallery {
    //            display: flex;
    //            flex-wrap: wrap;
    //            gap: 10px;
    //        }
    //        img {
    //            width: 200px;
    //            height: auto;
    //            border-radius: 8px;
    //            box-shadow: 0 4px 6px rgba(0,0,0,0.1);
    //        }
    //    </style>
    //</head>
    //<body>
    //    <div class="gallery">
    //        <img src="https://example.com/image1.jpg">
    //        <img src="https://example.com/image2.jpg">
    //        <img src="https://example.com/image3.jpg">
    //    </div>
    //</body>
    //</html>

    const string TopPart = 
        """
        <!-- Example Gallery HTML -->
        <html>
        <head>
            <style>
                .gallery {
                    display: flex;
                    flex-wrap: wrap;
                    gap: 10px;
                }
                img {
                    width: 200px;
                    height: auto;
                    border-radius: 8px;
                    box-shadow: 0 4px 6px rgba(0,0,0,0.1);
                }
            </style>
        </head>
        <body style="background-color:White;">
            <div class="gallery">
        """;

    const string BottomPart = 
        """
            </div>
        </body>
        </html>
        """;

    internal static string Generate(IEnumerable<Uri> uris) {
        var sb = new StringBuilder();
        sb.AppendLine(TopPart);
        foreach(var uri in uris) {
           var imageTag = $"        <img src=\"{uri}\">";
            sb.AppendLine(imageTag);
        }
        sb.AppendLine(BottomPart);
        return sb.ToString();
    }
}
