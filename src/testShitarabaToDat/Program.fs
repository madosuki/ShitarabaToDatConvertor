open System
open FSharp.Control.Tasks.V2
open CommandLine
open libShitarabaToDat.shitarabaToDatTools

type options = {
    [<Option("url", Required = true, HelpText = "Target URL")>] url : string
    [<Option("encoding", HelpText = "DAT File Encoding (e.g. sjis). Default value is utf-8. Used by System.Text.Encoding.GetEncoding()")>] encoding : string
}

let inline nullCheck x =
    if x <> null then
        Some(x)
    else
        None

let inline run (url: string) (encode: string option) =
    let c = shitarabaToDatClass(url)
    let r = c.htmlToDat.GetAwaiter().GetResult()

    match encode with
    | Some(encode) -> c.WriteToFile(encode)
    | None -> c.WriteToFile("utf-8")
    
[<EntryPoint>]
let main argv =
    let result = CommandLine.Parser.Default.ParseArguments<options>(argv)
    match result with
    | :? Parsed<options> as parsed -> run parsed.Value.url (nullCheck parsed.Value.encoding)
    | :? NotParsed<options> as notParsed -> printfn "%A" notParsed.Errors

    0 // return an integer exit code
