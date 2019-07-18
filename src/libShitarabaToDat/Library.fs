﻿namespace libShitarabaToDat

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

        member private this.removeEndOfBRTag(s: string) =
            let mutable current = s.Length - 1
            let mutable tmp = ""
            let mutable isChange = false
            let mutable isContinue = true
            printfn "%A" s

            while isContinue do
                tmp <- tmp + (string s.[current])

                if tmp.IndexOf(">rb<") > 0 && isChange = false then
                    isChange <- true
                    tmp <- ""

                current <- current - 1
                if current = -1 then
                    isContinue <- false
        
            let tmpArray = tmp.ToCharArray()
            Array.Reverse(tmpArray)
            new System.String(tmpArray)


        member private this.replaceAloneATag(s: string) =

            let parse = new HtmlParser()
            let parsed = parse.ParseDocument(s)
            let aList = parsed.QuerySelectorAll("a")

            let urlList = new List<string>()
            for i in urlList do
                urlList.Add(i)

            let mutable result = s
            if urlList.Count > 0 then
                let mutable tmp = s
                let mutable isContinue = true
                let mutable count = 0
                while isContinue = true do
                    let startPoint = s.IndexOf("<a>")
                    let endPoint = s.IndexOf("</a>") + 3

                    if startPoint = -1 then
                        isContinue <- false
                    else
                        tmp <- tmp.Substring(0, startPoint) + tmp.Substring(endPoint, tmp.Length - endPoint) + urlList.[count]
                        count <- count + 1
                result <- tmp

            result

        member private this.replaceRes(s: string) =
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

                let mutable tmp = s
                let mutable count = 0
                let mutable isContinue = true
                while isContinue do
                    let startPoint = tmp.IndexOf("<span class=\"res\">")
                    let endPoint = tmp.IndexOf("</span>") + 7

                    if startPoint = -1 then
                        isContinue <- false
                    else
                        // printfn "Count: %A, String Size: %A, Start: %A, End: %A" count tmp.Length startPoint (endPoint - 7)
                        let left = tmp.Substring(0, startPoint)
                        let right = tmp.Substring(endPoint, tmp.Length - endPoint)
                        tmp <- left + resNoList.[count] + right
                        count <- count + 1


                tmp
            else
                s

        member this.createDtList(dt : Dom.IHtmlCollection<Dom.IElement>) =
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

        member this.createDdList(dd : Dom.IHtmlCollection<Dom.IElement>) =
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
                                                                                                
                            tmpStr <- tmpStr + this.replaceRes(tmp)
                            isFirstLine <- false
                         else
                            let tmp = this.replaceRes(tmpList.[j])
                            tmpStr <- tmpStr + tmp

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
                            
                            let dtList = this.createDtList(dtCollection)
                            let ddList = this.createDdList(ddCollection)
            
                            for i in 0 .. (dtList.Count - 1) do
                                if i = 0 then
                                    datLineList.Add(dtList.[i] + ddList.[i] + "<>" + title)
                                else
                                    datLineList.Add(dtList.[i] + ddList.[i] + "<>")
                        }
        
                    return datLineList
                }
    
