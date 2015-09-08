namespace BucklingSprings.Aware.AddOns.Quote

open System
open System.ComponentModel
open System.Windows
open System.Windows.Controls
open System.Windows.Media
open System.Windows.Input

type DelegatingCommand(fx) =
    let ev = new Event<_,_>()
    interface ICommand with
        member x.CanExecute _ = true
        member x.Execute _ = fx ()
        [<CLIEvent>]
        member x.CanExecuteChanged = ev.Publish

type QuoteWindowViewModel() as vm =
    let mutable q = QuoteStore.currentQuote ()
    let ev = new Event<_,_>()

    let reshow () =
        q <- QuoteStore.currentQuote ()
        ev.Trigger(vm, PropertyChangedEventArgs("Author"))
        ev.Trigger(vm, PropertyChangedEventArgs("QuoteText"))
        ev.Trigger(vm, PropertyChangedEventArgs("TextColor"))
        ev.Trigger(vm, PropertyChangedEventArgs("Background"))


    member x.Author = q.author
    member x.QuoteText = q.text
    member x.TextColor = SolidColorBrush(q.textColor) :> Brush
    member x.Background = SolidColorBrush(q.backgroundColor) :> Brush
    member x.PreviousQuote = DelegatingCommand(fun _ -> QuoteStore.back (); reshow ())
    member x.NextQuote = DelegatingCommand(fun _ -> QuoteStore.forward (); reshow ())
    interface INotifyPropertyChanged with
        [<CLIEvent>]
        member x.PropertyChanged = ev.Publish

type QuoteWindow(quoteFile : string) as w =
    inherit Window()

    do
        QuoteStore.initialize quoteFile
        let content = Application.LoadComponent(Uri("/AwareQuote;component/QuoteWindow.xaml", UriKind.Relative)) :?> UserControl
        w.Content <- content
        w.DataContext <- QuoteWindowViewModel()
        w.WindowState <- WindowState.Maximized
