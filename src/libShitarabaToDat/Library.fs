namespace libShitarabaToDat

module shitarabaToDatTools =

    open System
    open System.IO
    open System.Collections.Generic
    open AngleSharp
    open AngleSharp.Html.Parser
    open FSharp.Control.Tasks.V2

    type shitarabaToDatClass(target: string) =
        let url = target
        let config = Configuration.Default.WithDefaultLoader()
        let context = BrowsingContext.New(config)
        // let mutable resultList = []
        let resultList = new List<string>()

        member this.WriteToFile(enc: string) =
            (*
            if resultList.Length > 0 then
                let mutable name = ""
                let mutable slashCount = 0
                for i in url do
                    if i = '/' then
                        slashCount <- slashCount + 1

                    if slashCount > 6 then
                        name <- name + (string i)

                use sw = new StreamWriter(name.Substring(1, name.Length - 2) + ".dat", false, System.Text.Encoding.GetEncoding(enc))

                for i: string in resultList do
                    sw.WriteLine(i)
            *)

            if resultList.Count > 0 then
                let mutable name = ""
                let mutable slashCount = 0
                for i in url do
                    if i = '/' then
                        slashCount <- slashCount + 1

                    if slashCount > 6 then
                        name <- name + (string i)
                                                                                                                         
                use sw = new StreamWriter(name.Substring(1, name.Length - 2) + ".dat", false, System.Text.Encoding.GetEncoding(enc))
                                                                                                                         
                for i : string in resultList do
                    sw.WriteLine(i)

        member private this.returnLine(s: string) =
            let mutable tmp = ""
            let mutable isChange = true
        
            for i in s do
                if isChange then
                    tmp <- tmp + (string i)
        
                if i = '>' then
                    isChange <- false
        
            tmp

        member private this.detectRes(s: string) =
            let mutable target = s
            let parser = new HtmlParser()
            let parsed = parser.ParseDocument(s)
            let spanList = parsed.QuerySelectorAll("span.res")
            for i in spanList do
                let a = i.QuerySelectorAll("a")
                if a.Length > 0 then
                    for j in a do
                        printfn "%A" j.TextContent
                        i.Remove()
            let text = parsed.Body.InnerHtml
            printfn "%A" text
    
        // Task
        member this.htmlToDat =
                task {
                    let! body = context.OpenAsync(url)
        
                    do! 
                        task{
                            let title = body.QuerySelectorAll("h1")
            
                            let dtCollection = body.QuerySelectorAll("dt")
                            let ddCollection = body.QuerySelectorAll("dd")
                            
                            (*
                            let mutable nameList = []
                            let mutable tripList = []
                            let mutable mailList = []
                            let mutable dateList = []
                            let mutable idList = []
                            let mutable textList = []
                            *)
    
                            let nameList = new List<string>()
                            let tripList = new List<string>()
                            let mailList = new List<string>()
                            let dateList = new List<string>()
                            let idList = new List<string>()
                            let textList = new List<string>()
            
                            if dtCollection.Length > 0 then
                                for i in dtCollection do
                                    let name = i.QuerySelectorAll("b")
                                    if name.Length > 0 then
                                        for j in name do
                                            // nameList <- nameList @ [j.TextContent;]
                                            nameList.Add(j.TextContent)
            
                                            let mutable trip = ""
                                            if j.NextSibling.TextContent.Length > 30 then
                                                let tmp = j.NextSibling.TextContent.Split '\n'
                                                trip <- tmp.[0]
            
                                            // tripList <- tripList @ [trip;]
                                            tripList.Add(trip)
            
                                    let tmpDate = i.ChildNodes.[0].NextSibling.NextSibling.TextContent
                                    let splited = tmpDate.Substring(15).Split '\n'
                                    let dateAndID = splited.[0].Split ' '
                                    // dateList <- dateList @ [dateAndID.[0] + " " + dateAndID.[1];]
                                    dateList.Add(dateAndID.[0] + " " + dateAndID.[1])
                                    // idList <- idList @ [dateAndID.[2];]
                                    idList.Add(dateAndID.[2])
            
                                    let aList = i.QuerySelectorAll("a")
                                    if aList.Length > 0 then
                                        let attr = aList.Item(0).Attributes
                                        let href = attr.Item(0).Value.Substring 7
                                        // mailList <- mailList @ [href;]
                                        mailList.Add(href)
                                    else
                                        // mailList <- mailList @ ["";]
                                        mailList.Add("")
    
                            let mutable count = 1
                            if ddCollection.Length > 0 then
                                for i in ddCollection do
                                    let mutable isFirstLine = true
            
                                    let tmpList = i.InnerHtml.Split '\n'
            
                                    count <- count + 1
            
                                    let mutable tmpStr = ""
                                    for j in 2 .. (tmpList.Length - 2) do
                                        if isFirstLine then
                                            // this.detectRes(tmpList.[j].Substring 13)
                                            let result: string = this.returnLine <| tmpList.[j].Substring 13
                                            tmpStr <- tmpStr + result
                                            isFirstLine <- false
                                        else
                                            // this.detectRes(tmpList.[j])
                                            let tmp = this.returnLine tmpList.[j]
                                            tmpStr <- tmpStr + tmp
            
                                    tmpStr <- tmpStr.Substring(0, (tmpStr.Length - 5))
                                    // textList <- textList @ [tmpStr;]
                                    textList.Add(tmpStr)
            
                            (*
                            for i in 0 .. (idList.Length - 1) do
                                match tripList.[i] with
                                | "" when i = 0 -> resultList <- resultList @ [nameList.[i] + "<>" + mailList.[i] + "<>" + dateList.[i] + " ID:" + idList.[i] + "<>" + textList.[i] + "<>" + title.Item(0).TextContent;]
                                | "" -> resultList <- resultList @ [nameList.[i] + "<>" + mailList.[i] + "<>" + dateList.[i] + " ID:" + idList.[i] + "<>" + textList.[i] + "<>"]
                                | _ when i = 0 -> resultList <- resultList @ [nameList.[i] + "</b>" + tripList.[i] + "<b><>" + mailList.[i] + "<>" + dateList.[i] + " ID:" + idList.[i] + "<>" + textList.[i] + "<>" + title.Item(0).TextContent;]
                                | _ -> resultList <- resultList @ [nameList.[i] + "</b>" + tripList.[i] + "<b><>" + mailList.[i] + "<>" + dateList.[i] + " ID:" + idList.[i] + "<>" + textList.[i] + "<>";]
                            *)
    
                            for i in 0 .. (idList.Count - 1) do
                                match tripList.[i] with
                                | "" when i = 0 -> resultList.Add(nameList.[i] + "<>" + mailList.[i] + "<>" + dateList.[i] + " ID:" + idList.[i] + "<>" + textList.[i] + "<>" + title.Item(0).TextContent)
                                | "" -> resultList.Add(nameList.[i] + "<>" + mailList.[i] + "<>" + dateList.[i] + " ID:" + idList.[i] + "<>" + textList.[i] + "<>")
                                | _ when i = 0 -> resultList.Add(nameList.[i] + "</b>" + tripList.[i] + "<b><>" + mailList.[i] + "<>" + dateList.[i] + " ID:" + idList.[i] + "<>" + textList.[i] + "<>" + title.Item(0).TextContent)
                                | _ -> resultList.Add(nameList.[i] + "</b>" + tripList.[i] + "<b><>" + mailList.[i] + "<>" + dateList.[i] + " ID:" + idList.[i] + "<>" + textList.[i] + "<>")
                        }
        
                    return resultList
                }
    
