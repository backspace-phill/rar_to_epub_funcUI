namespace CounterApp

open Avalonia
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.Themes.Fluent
open Avalonia.FuncUI.Hosts
open Avalonia.Controls
open Avalonia.FuncUI
open Avalonia.FuncUI.DSL
open Avalonia.Layout

module Main =
    open Avalonia.Platform.Storage
    open System.IO
    open System
    open System.Threading.Tasks

    let view () =
        Component(fun ctx ->
            let state = ctx.useState<Uri> null

            let selectFileAndConvert = async {
                let toplevel = TopLevel.GetTopLevel(ctx.control)
                let dialogOptions = 
                    let sp = FilePickerSaveOptions()
                    sp.FileTypeChoices <- [FilePickerFileType("Epub")]
                    sp
                let! file = toplevel.StorageProvider.SaveFilePickerAsync(dialogOptions) |> Async.AwaitTask
                match file with
                | null -> ()
                | file -> RarConverter.convertToEpub state.Current.AbsolutePath file.Path.AbsolutePath
            }

            let dialogOptions = 
                let op = FilePickerOpenOptions()
                op.FileTypeFilter <- [FilePickerFileType("Rar")]
                op.Title <- "Select a Rar file"
                op.AllowMultiple <- false
                op

            let openFileDialog = async {
                let toplev = TopLevel.GetTopLevel(ctx.control)
                let! files = toplev.StorageProvider.OpenFilePickerAsync(dialogOptions) |> Async.AwaitTask
                match files with
                | null -> printfn "There was an Error!"
                | _ -> 
                    let file = files.[0]
                    state.Set(file.Path)
                
            }

            DockPanel.create [
                DockPanel.children [
                        Button.create [
                            Button.dock Dock.Bottom
                            Button.content "Convert"
                            Button.onClick (fun _ ->  selectFileAndConvert |> Async.Start)
                            Button.horizontalAlignment HorizontalAlignment.Stretch
                            Button.horizontalContentAlignment HorizontalAlignment.Center
                            ]
                        Button.create [
                            Button.dock Dock.Bottom
                            Button.onClick (fun _ -> openFileDialog |> Async.Start)
                            Button.content "Select File"
                            Button.horizontalAlignment HorizontalAlignment.Stretch
                            Button.horizontalContentAlignment HorizontalAlignment.Center
                        ]
                    ]
            ]
        )

type MainWindow() =
    inherit HostWindow()
    do
        base.Title <- "Rar2Epub"
        base.Content <- Main.view ()
        base.Height <- 200.0
        base.Width <- 200.0

type App() =
    inherit Application()

    override this.Initialize() =
        this.Styles.Add (FluentTheme())
        this.RequestedThemeVariant <- Styling.ThemeVariant.Default

    override this.OnFrameworkInitializationCompleted() =
        match this.ApplicationLifetime with
        | :? IClassicDesktopStyleApplicationLifetime as desktopLifetime ->
            desktopLifetime.MainWindow <- MainWindow()
        | _ -> ()

module Program =

    [<EntryPoint>]
    let main(args: string[]) =
        AppBuilder
            .Configure<App>()
            .UsePlatformDetect()
            .UseSkia()
            .StartWithClassicDesktopLifetime(args)
