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
        // let mutable datLineList = []
        let datLineList = new List<string>()

        member this.WriteToFile(enc: string) =
            if datLineList.Count > 0 then
                let mutable name = ""
                let mutable slashCount = 0
                for i in url do
                    if i = '/' then
                        slashCount <- slashCount + 1

                    if slashCount > 6 then
                        name <- name + (string i)
                                                                                                                         
                use sw = new StreamWriter(name.Substring(1, name.Length - 2) + ".dat", false, System.Text.Encoding.GetEncoding(enc))
                                                                                                                         
                for i : string in datLineList do
                    sw.WriteLine(i)

        member private this.replaceAloneATag(s: byref<string>) =
            let parse = new HtmlParser()
            let parsed = parse.ParseDocument(s)
            let aList = parsed.QuerySelectorAll("a")

            if aList.Length > 0 then
                let urlList = new List<string>()
                for i in urlList do
                    urlList.Add(i)

                if urlList.Count > 0 then
                    let mutable isContinue = true
                    let mutable count = 0
                    while isContinue = true do
                        let startPoint = s.IndexOf("<a>")
                        let endPoint = s.IndexOf("</a>") + 4
    
                        if startPoint = -1 then
                            isContinue <- false
                        else
                            s <- s.Substring(0, startPoint) + urlList.[count] + s.Substring(endPoint, s.Length - endPoint)
                            count <- count + 1
    
        member private this.replaceRes(s: byref<string>) =
            let parser = new HtmlParser()
            let parsed = parser.ParseDocument(s)
            let spanList = parsed.QuerySelectorAll("span.res")

            if spanList.Length > 0 then
                let resNoList = new List<string>()
                for i in spanList do
                    let a = i.QuerySelectorAll("a")
                    if a.Length > 0 then
                        for j in a do
                            let tmp = j.TextContent.Replace(">>", "&gt;&gt;")
                            resNoList.Add(tmp)

                let mutable count = 0
                let mutable isContinue = true
                while isContinue do
                    let startPoint = s.IndexOf("<span class=\"res\">")
                    let endPoint = s.IndexOf("</span>") + 7

                    if startPoint = -1 then
                        isContinue <- false
                    else
                        // printfn "Count: %A, String Size: %A, Start: %A, End: %A" count tmp.Length startPoint (endPoint - 7)
                        let left = s.Substring(0, startPoint)
                        let right = s.Substring(endPoint, s.Length - endPoint)
                        s <- left + resNoList.[count] + right
                        count <- count + 1

        member private this.createDtList (dt : inref<Dom.IHtmlCollection<Dom.IElement>>) =
            let nameList = new List<string>()
            let tripList = new List<string>()
            let mailList = new List<string>()
            let dateList = new List<string>()
            let idList = new List<string>()

            let dtList = new List<string>()
            if dt.Length > 0 then
                for i in dt do
                    let name = i.QuerySelectorAll("b")
                    if name.Length > 0 then
                        for j in name do
                            nameList.Add(j.TextContent)
        
                            let mutable trip = ""
                            if j.NextSibling.TextContent.Length > 30 then
                                let tmp = j.NextSibling.TextContent.Split '\n'
                                trip <- tmp.[0]
        
                            tripList.Add(trip)
        
                            let tmpDate = i.ChildNodes.[0].NextSibling.NextSibling.TextContent
                            let splited = tmpDate.Substring(15).Split '\n'
                            let dateAndID = splited.[0].Split ' '
                            dateList.Add(dateAndID.[0] + " " + dateAndID.[1])
                            idList.Add(dateAndID.[2])
        
                            let aList = i.QuerySelectorAll("a")
                            if aList.Length > 0 then
                                let attr = aList.Item(0).Attributes
                                let href = attr.Item(0).Value.Substring 7
                                mailList.Add(href)
                            else
                                mailList.Add("")

                for i in 0 .. (idList.Count - 1) do
                    match tripList.[i] with
                    | "" -> dtList.Add(nameList.[i] + "<>" + mailList.[i] + "<>" + dateList.[i] + " ID:" + idList.[i] + "<>")
                    | _ -> dtList.Add(nameList.[i] + "</b>" + tripList.[i] + "<b><>" + mailList.[i] + "<>" + dateList.[i] + " ID:" + idList.[i]+ "<>")
            dtList

        member this.createDdList(dd : inref<Dom.IHtmlCollection<Dom.IElement>>) =
            let ddList = new List<string>()
            let mutable count = 1

            if dd.Length > 0 then
                for i in dd do
                    let mutable isFirstLine = true

                    let tmpList = (i.InnerHtml.Split '\n').[2..]

                    count <- count + 1

                    let mutable tmpStr = ""
                    for j in 0 .. (tmpList.Length - 1) do
                        if isFirstLine then
                            let mutable tmp = ""
                            if tmpList.[j].Length > 13 then
                                tmp <- tmpList.[j].Substring 13
                            else
                                tmp <- tmpList.[j]
                            
                            this.replaceRes(&tmp)
                            this.replaceAloneATag(&tmp)
                            tmpStr <- tmpStr + tmp
                            isFirstLine <- false
                         else
                            this.replaceRes(&tmpList.[j])
                            this.replaceAloneATag(&tmpList.[j])
                            tmpStr <- tmpStr + tmpList.[j]

                    ddList.Add(tmpStr.Substring(0, tmpStr.Length - 25))
            ddList
    
        // Task
        member this.htmlToDat =
                task {
                    let! body = context.OpenAsync(url)
        
                    do! 
                        task{
                            let title = body.QuerySelectorAll("h1").Item(0).TextContent
            
                            let dtCollection = body.QuerySelectorAll("dt")
                            let ddCollection = body.QuerySelectorAll("dd")
                            
                            let dtList = this.createDtList(&dtCollection)
                            let ddList = this.createDdList(&ddCollection)
            
                            for i in 0 .. (dtList.Count - 1) do
                                if i = 0 then
                                    datLineList.Add(dtList.[i] + ddList.[i] + "<>" + title)
                                else
                                    datLineList.Add(dtList.[i] + ddList.[i] + "<>")
                        }
        
                    return true
                }
    
