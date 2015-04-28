namespace BucklingSprings.Aware.AddOns.Quote

open System
open System.Diagnostics
open System.IO
open System.Xml
open System.Windows.Media


module QuoteStore =

    let colors = [|
                    Color.FromRgb(223uy, 107uy, 50uy)
                    Color.FromRgb(191uy, 203uy, 67uy)
                    Color.FromRgb(112uy, 93uy, 149uy)
                    Color.FromRgb(106uy, 197uy, 101uy)
                    Color.FromRgb(4uy, 104uy, 138uy)
                    Color.FromRgb(204uy, 83uy, 56uy)
                 |]

    type Quote = {
        text : string
        author : string
        textColor : Color
        backgroundColor : Color
    }

    type StoreState = {
        quotes: Quote List
        position : int
    }


    let cannedQuotes = [
            ("ACTION MAY NOT ALWAYS BRING HAPPINESS; BUT THERE IS NO HAPPINESS WITHOUT ACTION", "Benjamin Disraeli")
            ("I MUST LOSE MYSELF IN ACTION, LEST I WITHER IN DESPAIR.", "Alfred Lord Tennyson")
            ("A REAL DECISION IS MEASURED BY THE FACT THAT YOU'VE TAKEN A NEW ACTION. IF THERE'S NO ACTION, YOU HAVEN'T TRULY DECIDED.", "Tony Robbins")
            ("DO YOU WANT TO KNOW WHO YOU ARE? DON'T ASK. ACT! ACTION WILL DELINEATE AND DEFINE YOU.", "Thomas Jefferson")
            ("DETERMINE NEVER TO BE IDLE. NO PERSON WILL HAVE OCCASION TO COMPLAIN OF THE WANT OF TIME WHO NEVER LOSES ANY. IT IS WONDERFUL HOW MUCH MAY BE DONE IF WE ARE ALWAYS DOING.", "Thomas Jefferson")
        ]

    let fromText xs =
        let colorCount = Array.length colors
        let r = new Random() 
        let randomColor () =
            colors.[(r.Next(colorCount-1))]
        xs
            |> List.map (fun (t,a) -> 
                             {
                                text = t
                                author = a
                                textColor = Colors.White
                                backgroundColor = randomColor ()
                             })

    let mutable storeState = {
            position = 0
            quotes =  fromText cannedQuotes
    }

    let fromFile (f : string) =
        let doc = XmlDocument()
        doc.Load(f)
        let qs = doc.GetElementsByTagName("quote")
        let quote i =
            let node = qs.ItemOf i
            (
                node.SelectSingleNode("text").InnerText.Trim(),
                node.SelectSingleNode("author").InnerText.Trim()
            )
        [for i in 0..(qs.Count-1) do yield quote i ]
            

        
        

    let initialize quoteFile =
        try
            if File.Exists quoteFile then
                let quotesFromFile = fromFile quoteFile
                let newState = {
                                    position = 0
                                    quotes =  fromText quotesFromFile
                               }
                if List.length quotesFromFile > 0 then
                    storeState <- newState
                else
                    Debug.WriteLine("No quote found in file")
        with
            | x -> Debug.WriteLine (x.ToString())
        
        


    let currentQuote () = storeState.quotes
                            |> Seq.nth (storeState.position)


    let back () = 
        let pos = storeState.position
        let len = List.length storeState.quotes
        let newPosition = if pos > 0 then (pos - 1) else (len - 1)
        storeState <- {storeState with position = newPosition }

    let forward () =
        let newPosition = (storeState.position + 1) % (List.length storeState.quotes)
        storeState <- {storeState with position = newPosition }
    