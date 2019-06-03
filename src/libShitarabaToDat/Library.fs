namespace libShitarabaToDat

module shitarabaToDatTools =

    open System
    open AngleSharp
    open FSharp.Control.Tasks.V2

    type htmlToDatClass(target: string) =
        let url = target
        let config = Configuration.Default.WithDefaultLoader()
        let context = BrowsingContext.New(config)
        let mutable resultList = []

        member private this.returnLine (s: string) =
            let mutable tmp = ""
            let mutable isChange = true
        
            for i in s do
                if isChange then
                    tmp <- tmp + (string i)
        
                if i = '>' then
                    isChange <- false
        
            tmp
    
        // Task
        member this.htmlToDat =
                task {
                    let! body = context.OpenAsync(url)
        
                    do 
                        let title = body.QuerySelectorAll("h1")
        
                        let dtCollection = body.QuerySelectorAll("dt")
                        let ddCollection = body.QuerySelectorAll("dd")
                        
                        let mutable nameList = []
                        let mutable tripList = []
                        let mutable mailList = []
                        let mutable dateList = []
                        let mutable idList = []
                        let mutable textList = []
        
                        if dtCollection.Length > 0 then
                            for i in dtCollection do
                                let name = i.QuerySelectorAll("b")
                                if name.Length > 0 then
                                    for j in name do
                                        nameList <- nameList @ [j.TextContent;]
        
                                        let mutable trip = ""
                                        if j.NextSibling.TextContent.Length > 30 then
                                            let tmp = j.NextSibling.TextContent.Split '\n'
                                            trip <- tmp.[0]
        
                                        tripList <- tripList @ [trip;]
        
                                let tmpDate = i.ChildNodes.[0].NextSibling.NextSibling.TextContent
                                let splited = tmpDate.Substring(15).Split '\n'
                                let dateAndID = splited.[0].Split ' '
                                dateList <- dateList @ [dateAndID.[0] + " " + dateAndID.[1];]
                                idList <- idList @ [dateAndID.[2];]
        
                                let aList = i.QuerySelectorAll("a")
                                if aList.Length > 0 then
                                    let attr = aList.Item(0).Attributes
                                    let href = attr.Item(0).Value.Substring 7
                                    mailList <- mailList @ [href;]
                                else
                                    mailList <- mailList @ ["";]
        
                        let mutable count = 1
                        if ddCollection.Length > 0 then
                            for i in ddCollection do
                                let mutable isFirstLine = true
        
                                let tmpList = i.InnerHtml.Split '\n'
        
                                count <- count + 1
        
                                let mutable tmpStr = ""
                                for j in 2 .. (tmpList.Length - 2) do
                                    if isFirstLine then
                                        let tmp = tmpList.[j].Substring 13
                                        let result: string = this.returnLine(tmp)
                                        tmpStr <- tmpStr + result
                                        isFirstLine <- false
                                    else
                                        let tmp = this.returnLine tmpList.[j]
                                        tmpStr <- tmpStr + tmp
        
                                tmpStr <- tmpStr.Substring(0, (tmpStr.Length - 4))
                                textList <- textList @ [tmpStr;]
        
        
                        for i in 0 .. (idList.Length - 1) do
                            match tripList.[i] with
                            | "" when i = 0 -> resultList <- resultList @ [nameList.[i] + "<>" + mailList.[i] + "<>" + dateList.[i] + " ID:" + idList.[i] + "<>" + textList.[i] + "<>" + title.Item(0).TextContent;]
                            | "" -> resultList <- resultList @ [nameList.[i] + "<>" + mailList.[i] + "<>" + dateList.[i] + " ID:" + idList.[i] + "<>" + textList.[i] + "<>"]
                            | _ when i = 0 -> resultList <- resultList @ [nameList.[i] + "</b>" + tripList.[i] + "<b><>" + mailList.[i] + "<>" + dateList.[i] + " ID:" + idList.[i] + "<>" + textList.[i] + "<>" + title.Item(0).TextContent;]
                            | _ -> resultList <- resultList @ [nameList.[i] + "</b>" + tripList.[i] + "<b><>" + mailList.[i] + "" + dateList.[i] + " ID:" + idList.[i] + "<>" + textList.[i] + "<>";]
        
                    return resultList
                }
    
