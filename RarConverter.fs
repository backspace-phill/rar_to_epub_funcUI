module RarConverter
open SharpCompress.Archives.Rar
open SharpCompress.Archives
open SharpCompress.Writers
open System.IO
open SharpCompress.Common

let convertToEpub (pathOfRarFile: string) (outputPath: string) =
    let memStram = new MemoryStream()
    let temDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName())
    use archive = RarArchive.Open(pathOfRarFile)
    archive.ExtractToDirectory(temDir)
    use epub = SharpCompress.Archives.Zip.ZipArchive.Create()
    epub.AddAllFromDirectory(temDir)
    let writeroptions = new WriterOptions(CompressionType.Deflate)
    epub.SaveTo(outputPath, writeroptions)
    Directory.Delete(temDir, true)
    ()