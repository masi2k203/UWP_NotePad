using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.ApplicationModel.DataTransfer;
using System.Collections.ObjectModel;

// 空白ページの項目テンプレートについては、https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x411 を参照してください

namespace MemoApp
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        ObservableCollection<FontFamily> fonts = new ObservableCollection<FontFamily>();
        private int useFontSize = 0;
        public MainPage()
        {
            this.InitializeComponent();
            fonts.Add(new FontFamily("Yu Gothic"));
            fonts.Add(new FontFamily("メイリオ"));
        }

        /// <summary>
        /// [名前を付けて保存]ボタンを押した時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var filePicker = new FileSavePicker();

            // 初期フォルダを「ドキュメント」フォルダにする
            filePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

            // 既定の拡張子を".txt"にする
            filePicker.DefaultFileExtension = ".txt";

            // サポートするファイルの種類を".txt"にする
            filePicker.FileTypeChoices.Add("テキスト", new List<string>() { ".txt" });

            // ファイル名の候補を「新規メモ」にする
            filePicker.SuggestedFileName = "新規メモ";

            StorageFile file = await filePicker.PickSaveFileAsync();

            // [保存]ボタンが押された場合(fileがnull以外)の処理
            if (file != null)
            {
                // txtMemo.Textの内容をファイルに保存する
                await FileIO.WriteTextAsync(file, txtMemo.Text);
            }
        }

        /// <summary>
        /// [開く]ボタンを押した時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            var filePicer = new FileOpenPicker();

            // 初期フォルダを「ドキュメント」フォルダにする
            filePicer.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

            // サポートするファイルの種類を".txt"にする
            filePicer.FileTypeFilter.Add(".txt");
            StorageFile file = await filePicer.PickSingleFileAsync();

            // [開く]ボタンが押された場合(fileがnull以外)の処理
            if (file != null)
            {
                // ファイルからテキストを読み込む
                try
                {
                    string text = await FileIO.ReadTextAsync(file);
                    txtMemo.Text = text;
                }
                catch (FileNotFoundException fnfe)
                {
                    // ファイルが見つからなかった場合
                    MessageDialog dialog = new MessageDialog(fnfe.Message, "エラー");
                    await dialog.ShowAsync();
                }
            }
        }

        /// <summary>
        /// [元に戻す]ボタンを押した時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUndo_Click(object sender, RoutedEventArgs e)
        {
            // 直前の操作を元に戻す
            txtMemo.Undo();
        }

        /// <summary>
        /// [コピー]ボタンを押した時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            DataPackage dataPackage = new DataPackage();

            // クリップボード操作をコピーに設定する
            dataPackage.RequestedOperation = DataPackageOperation.Copy;

            // txtMemoで選択されているテキストをクリップボードにセットする
            dataPackage.SetText(txtMemo.SelectedText);

            // クリップボードにデータをセットする
            Clipboard.SetContent(dataPackage);

            // 再度txtMemoにフォーカスを当てる
            txtMemo.Focus(FocusState.Programmatic);
        }

        /// <summary>
        /// [切り取り]ボタンを押した時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCut_Click(object sender, RoutedEventArgs e)
        {
            DataPackage dataPackage = new DataPackage();
            int startPos = txtMemo.SelectionStart;
            int selectedLen = txtMemo.SelectionLength;

            // クリップボード操作をコピーに設定する
            dataPackage.RequestedOperation = DataPackageOperation.Move;

            // txtMemoで選択されているテキストをクリップボードにセットする
            dataPackage.SetText(txtMemo.SelectedText);

            // クリップボードにデータをセットする
            Clipboard.SetContent(dataPackage);

            // 選択された文字列を切り取って新しい文字列を作成する
            string strNewMemo = txtMemo.Text.Substring(0, startPos) + txtMemo.Text.Substring(startPos + selectedLen);

            // 新しく作成した文字列をtxtMemoにセット
            txtMemo.Text = strNewMemo;

            // 切り取り開始位置にキャレットをセットする
            txtMemo.SelectionStart = startPos;

            // 再度txtMemoにフォーカスを当てる
            txtMemo.Focus(FocusState.Programmatic);
        }

        /// <summary>
        /// [貼り付け]ボタンを押した時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnPaste_Click(object sender, RoutedEventArgs e)
        {
            // クリップボードからデータを取得する
            DataPackageView dataPackageView = Clipboard.GetContent();
            string strMemo = await dataPackageView.GetTextAsync();

            // 取得した文字をtxtMemoのキャレットの位置に挿入する
            txtMemo.Text = txtMemo.Text.Insert(txtMemo.SelectionStart, strMemo);

            // 再度txtMemoにフォーカスを当てる
            txtMemo.Focus(FocusState.Programmatic);
        }

        /// <summary>
        /// [新規作成]を押した時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnNew_Click(object sender, RoutedEventArgs e)
        {
            this.digConfirm.Content = "変更内容は破棄されます。";

            var result = await this.digConfirm.ShowAsync();

            // 「はい」が押された場合
            if (result == ContentDialogResult.Primary)
            {
                // 表示内容をクリアする
                txtMemo.Text = string.Empty;
            }

            // 再度txtMemoにフォーカスを当てる
            txtMemo.Focus(FocusState.Programmatic);
        }

        /// <summary>
        /// バージョン情報表示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnInfo_Click(object sender, RoutedEventArgs e)
        {
            this.infodigConfirm.Content = "メモ帳(UWP)\nバージョン：0.3.a02(added:2)";

            var result = await this.infodigConfirm.ShowAsync();

            // 再度txtMemoにフォーカスを当てる
            txtMemo.Focus(FocusState.Programmatic);
        }

        /// <summary>
        /// 設定画面への遷移
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSetting_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(SettingPage));
        }

        /// <summary>
        /// フォントサイズのインクリメント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnFontSizeIncrease_Click(object sender, RoutedEventArgs e)
        {
            txtMemo.FontSize++;
        }

        /// <summary>
        /// フォントサイズのデクリメント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnFontSizeDecrease_Click(object sender, RoutedEventArgs e)
        {
            useFontSize = (int)txtMemo.FontSize;

            if (useFontSize > 4)
            {
                txtMemo.FontSize--;
            }
        }

        /// <summary>
        /// Textが変化するときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtMemo_TextChanged(object sender, TextChangedEventArgs e)
        {
            int txtLength = txtMemo.SelectionStart;
            CaretPosition.Text = string.Format($"文字数 : {txtLength}");
        }

        /// <summary>
        /// Sliderによるフォントサイズの変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sliderFontSize_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            
        }

        /// <summary>
        /// [フォント設定]を押した時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnFontStyle_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(FontSettingPage));
        }
    }
}
