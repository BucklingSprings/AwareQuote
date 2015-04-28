namespace BucklingSprings.Aware.AddOns.Quote

open System
open System.Threading
open System.Windows

open Nessos.UnionArgParser

module Program =

    type Arguments =
        | Minutes of int
        | More_Or_Less of string
        | Threshold of int
        | Quote_file of string
    with
        interface IArgParserTemplate with
            member s.Usage = 
                match s with
                    | Arguments.Minutes _ -> "Minutes spent on task so far."
                    | Arguments.More_Or_Less _ -> "More, Less or Neutral. Should you be doing More or Less of this task."
                    | Arguments.Threshold _ -> "Launch the application once these many minutes have been spent on a task marked as lessof. Defaults to 15 minutes."
                    | Arguments.Quote_file _ -> "File to load the quotes from. Defaults to AwareQuotes.xml"

    [<EntryPoint>]
    [<STAThread>]
    let main argv = 
        // Use a named mutex to ensure only one instance of this appication is running
        let createdNew = ref false
        use mutex = new Mutex(true, "BD3B9736-9E64-407E-B11E-7CAB8E293550", createdNew)
        if !createdNew then
            let parser = UnionArgParser.Create<Arguments>()
            let results = parser.Parse(argv)

            // The application only shows an interface if
            // no arguments are provided for the More_or_less argument
            // or more than 'Threshold' minutes have been spent on a "Less Of" task.
            let launch = if results.Contains(<@ Arguments.More_Or_Less  @>) then
                            let t = results.GetResult(<@ Arguments.Threshold  @>, 15)
                            let m = results.GetResult(<@ Arguments.Minutes @>, 0)
                            let moreOrLess = results.GetResult(<@ Arguments.More_Or_Less @>, "Neutral")
                            let lessOf = moreOrLess.Equals("Less", StringComparison.CurrentCultureIgnoreCase)
                            lessOf && (m > t)
                         else
                            true
            if launch then
                let quoteFile = results.GetResult(<@ Arguments.Quote_file  @>, "AwareQuotes.xml")
                let app = Application()
                app.Run(QuoteWindow(quoteFile))
            else
                -1
        else
            -2
