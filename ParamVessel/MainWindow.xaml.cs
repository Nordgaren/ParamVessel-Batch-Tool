﻿using MahApps.Metro.Controls;
using MeowDSIO;
using MeowDSIO.DataFiles;
using MeowDSIO.DataTypes.PARAM;
using MeowDSIO.DataTypes.PARAMDEF;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MeowsBetterParamEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            //DEBUG
            //PARAMDATA.DEBUG_RestoreBackupsLoadResave();

#if REMASTER
            Title += " (DS1R-Only Version)";
#else
            Title += " (PTDE-Only Version)";
#endif
        }

        private void SetLoadingMode(bool isLoading)
        {
            MainGrid.Opacity = isLoading ? 0.25 : 1;
            MainGrid.IsEnabled = !isLoading;

            LoadingTextBox.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;

            Mouse.OverrideCursor = isLoading ? Cursors.Wait : null;
        }

        private void SetSavingMode(bool isLoading)
        {
            MainGrid.Opacity = isLoading ? 0.25 : 1;
            MainGrid.IsEnabled = !isLoading;

            SavingTextBox.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;

            Mouse.OverrideCursor = isLoading ? Cursors.Wait : null;
        }

        private async Task BrowseForInterrootDialog(Action<bool> setIsLoading)
        {
#if REMASTER
            var browseDialog = new OpenFileDialog()
            {
                AddExtension = false,
                CheckFileExists = true,
                CheckPathExists = true,
                Multiselect = false,
                FileName = "DarkSoulsRemastered.exe",
                Filter = "Dark Souls Remastered EXEs (Usually DarkSoulsRemastered.exe)|*.exe",
                ShowReadOnly = false,
                Title = "Choose your DarkSoulsRemastered.exe file...",
                ValidateNames = true
            };
#else
            var browseDialog = new OpenFileDialog()
            {
                AddExtension = false,
                CheckFileExists = true,
                CheckPathExists = true,
                Multiselect = false,
                FileName = "DARKSOULS.exe",
                Filter = "Dark Souls: PTDE EXEs (Usually DARKSOULS.exe)|*.exe",
                ShowReadOnly = false,
                Title = "Choose your DARKSOULS.exe file...",
                ValidateNames = true
            };
#endif



            if ((browseDialog.ShowDialog() ?? false) == true)
            {
                var interrootDir = new FileInfo(browseDialog.FileName).DirectoryName;
                if (CheckInterrotDirValid(interrootDir))
                {
                    PARAMDATA.Config.InterrootPath = interrootDir;
                    PARAMDATA.SaveConfig();
                    await PARAMDATA.LoadParamsInOtherThread(setIsLoading);
                }
                else
                {
                    var sb = new StringBuilder();

#if REMASTER
                    sb.AppendLine(@"Directory of EXE chosen did not include the following directories/files which are required:");
                    sb.AppendLine(@" - '.\param\DrawParam\'");
                    sb.AppendLine(@" - '.\param\GameParam\'");
                    sb.AppendLine(@" - '.\param\GameParam\GameParam.parambnd.dcx'");
                    sb.AppendLine(@" - '.\paramdef\paramdef.paramdefbnd.dcx'");
#else
                    sb.AppendLine(@"Directory of EXE chosen did not include the following directories/files which are required:");
                    sb.AppendLine(@" - '.\param\DrawParam\'");
                    sb.AppendLine(@" - '.\param\GameParam\'");
                    sb.AppendLine(@" - '.\param\GameParam\GameParam.parambnd'");
                    sb.AppendLine(@" - '.\paramdef\paramdef.paramdefbnd'");

                    if (CheckIfUdsfmIsProbablyNotInstalled(interrootDir))
                    {
                        sb.AppendLine();
                        sb.AppendLine();
                        sb.AppendLine("Warning: This installation does not appear to be unpacked with " +
                            "UnpackDarkSoulsForModding because it meets one or more of the " +
                            "criteria below:");

                        sb.AppendLine(@" - '.\unpackDS-backup' does not exist.");
                        sb.AppendLine(@" - '.\dvdbnd0.bdt' exists.");
                        sb.AppendLine(@" - '.\dvdbnd1.bdt' exists.");
                        sb.AppendLine(@" - '.\dvdbnd2.bdt' exists.");
                        sb.AppendLine(@" - '.\dvdbnd3.bdt' exists.");
                        sb.AppendLine(@" - '.\dvdbnd0.bhd5' exists.");
                        sb.AppendLine(@" - '.\dvdbnd1.bhd5' exists.");
                        sb.AppendLine(@" - '.\dvdbnd2.bhd5' exists.");
                        sb.AppendLine(@" - '.\dvdbnd3.bhd5' exists.");
                    }
#endif





                    MessageBox.Show(
                        sb.ToString(), 
                        "Invalid Directory", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Error);

                }
            }
        }

        private bool CheckIfUdsfmIsProbablyNotInstalled(string dir)
        {
            return !Directory.Exists(IOHelper.Frankenpath(dir, @"unpackDS-backup")) ||
                File.Exists(IOHelper.Frankenpath(dir, @"dvdbnd0.bdt")) ||
                File.Exists(IOHelper.Frankenpath(dir, @"dvdbnd1.bdt")) ||
                File.Exists(IOHelper.Frankenpath(dir, @"dvdbnd2.bdt")) ||
                File.Exists(IOHelper.Frankenpath(dir, @"dvdbnd3.bdt")) ||
                File.Exists(IOHelper.Frankenpath(dir, @"dvdbnd0.bhd5")) ||
                File.Exists(IOHelper.Frankenpath(dir, @"dvdbnd1.bhd5")) ||
                File.Exists(IOHelper.Frankenpath(dir, @"dvdbnd2.bhd5")) ||
                File.Exists(IOHelper.Frankenpath(dir, @"dvdbnd3.bhd5"));
        }

        private bool CheckInterrotDirValid(string dir)
        {
#if REMASTER
            return
                Directory.Exists(IOHelper.Frankenpath(dir, @"param\DrawParam")) &&
                Directory.Exists(IOHelper.Frankenpath(dir, @"param\GameParam")) &&
                Directory.Exists(IOHelper.Frankenpath(dir, @"paramdef")) &&
                File.Exists(IOHelper.Frankenpath(dir, @"param\GameParam\GameParam.parambnd.dcx")) &&
                File.Exists(IOHelper.Frankenpath(dir, @"paramdef\paramdef.paramdefbnd.dcx"));
#else
            return
                Directory.Exists(IOHelper.Frankenpath(dir, @"param\DrawParam")) &&
                Directory.Exists(IOHelper.Frankenpath(dir, @"param\GameParam")) &&
                Directory.Exists(IOHelper.Frankenpath(dir, @"paramdef")) &&
                File.Exists(IOHelper.Frankenpath(dir, @"param\GameParam\GameParam.parambnd")) &&
                File.Exists(IOHelper.Frankenpath(dir, @"paramdef\paramdef.paramdefbnd"));
#endif

        }

        //private void RANDOM_DEBUG_TESTING()
        //{
        //    //var uniqueInternalDataTypes = new List<string>();
        //    //foreach (var p in PARAMDATA.ParamDefs)
        //    //{
        //    //    string testDir = IOHelper.Frankenpath(Environment.CurrentDirectory, "VERBOSE_DUMP");

        //    //    if (!Directory.Exists(testDir))
        //    //        Directory.CreateDirectory(testDir);

        //    //    string testFileName = IOHelper.Frankenpath(testDir, p.Key + ".txt");

        //    //    var sb = new StringBuilder();

        //    //    foreach (var e in p.Value.Entries)
        //    //    {
        //    //        sb.AppendLine($"{e.Name}:");
        //    //        sb.AppendLine($"\tID: {e.ID}");
        //    //        sb.AppendLine($"\tInternal Value Type: {e.InternalValueType}");
        //    //        sb.AppendLine($"\tDefault: {e.DefaultValue}");
        //    //        sb.AppendLine($"\t[PARAM MAN] Display Name: {e.DisplayName}:");
        //    //        sb.AppendLine($"\t[PARAM MAN] Description: {e.Description}");
        //    //        sb.AppendLine($"\t[PARAM MAN] Min: {e.Min}");
        //    //        sb.AppendLine($"\t[PARAM MAN] Max: {e.Max}");
        //    //        sb.AppendLine($"\t[PARAM MAN] Incrementation: {e.Increment}");
        //    //        sb.AppendLine($"\t[PARAM MAN] GUI Value Format: \"{e.GuiValueStringFormat}\"");
        //    //        sb.AppendLine($"\t[PARAM MAN] GUI Value Type: {e.GuiValueType}");
        //    //        sb.AppendLine($"\t[PARAM MAN] GUI Value Mode: {e.GuiValueDisplayMode}");
        //    //        sb.AppendLine($"\t[PARAM MAN] GUI Value Size: {e.GuiValueByteCount}");
        //    //        sb.AppendLine();
        //    //    }

        //    //    File.WriteAllText(testFileName, sb.ToString());
        //    //}

        //    var sb = new StringBuilder();

        //    foreach (var p in PARAMDATA.Params)
        //    {
        //        int defSize = p.Value.AppliedPARAMDEF.CalculateEntrySize();

        //        if (p.Value._debug_calculatedEntrySize != defSize)
        //        {
        //            sb.AppendLine($"Entry size check fail - {p.BNDName} - {p.Value.Name} - {p.Value._debug_calculatedEntrySize} (Def: {defSize})");

        //            //foreach (var e in p.Value.AppliedPARAMDEF.Entries)
        //            //{

        //            //    sb.AppendLine($"\tDef Entry size check fail ({e.Name}) - BitCount = {e.ValueBitCount}, DispVarBytes*8 = {(e.GuiValueByteCount * 8)}");
        //            //}
        //        }
        //    }

        //    Console.WriteLine(sb.ToString());

        //    Console.WriteLine();
        //}

        public PARAM SelectedParam
        {
            get
            {
                if (MainTabs.SelectedValue != null)
                    return (MainTabs.SelectedItem as PARAMRef).Value;
                else
                    return null;
            }
        }

        private void UltraSuperMegaDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {

        }

        private void UltraSuperMegaDataGrid_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void CopyJPText_Click(object sender, RoutedEventArgs e)
        {
            var sb = new StringBuilder();

            if (ParamManStyleDataGrid.SelectedItems.Count > 1)
            {
                for (int i = 0; i < ParamManStyleDataGrid.SelectedItems.Count; i++)
                {
                    if (i > 0)
                        sb.AppendLine();
                    var cell = (ParamCellValueRef)ParamManStyleDataGrid.SelectedItems[i];
                    sb.AppendLine(cell.Def.Name);
                    AppendCellInfoToStringBuilder(cell, sb, jpTextOnly: true, indentPrefix: "\t");
                }
            }
            else
            {
                AppendCellInfoToStringBuilder((sender as MenuItem).DataContext as ParamCellValueRef, sb, jpTextOnly: true);
            }

            Clipboard.SetText(sb.ToString());
        }

        private void CopyAllInfo_Click(object sender, RoutedEventArgs e)
        {
            var sb = new StringBuilder();

            if (ParamManStyleDataGrid.SelectedItems.Count > 1)
            {
                for (int i = 0; i < ParamManStyleDataGrid.SelectedItems.Count; i++)
                {
                    if (i > 0)
                        sb.AppendLine();
                    var cell = (ParamCellValueRef)ParamManStyleDataGrid.SelectedItems[i];
                    sb.AppendLine(cell.Def.Name);
                    AppendCellInfoToStringBuilder(cell, sb, jpTextOnly: false, indentPrefix: "\t");
                }
            }
            else
            {
                AppendCellInfoToStringBuilder((sender as MenuItem).DataContext as ParamCellValueRef, sb, jpTextOnly: false);
            }

            Clipboard.SetText(sb.ToString());
        }

        private void AppendCellInfoToStringBuilder(ParamCellValueRef cell, StringBuilder sb, bool jpTextOnly, string indentPrefix = "")
        {
            sb.AppendLine($"{indentPrefix}{cell.Def.DisplayName}:");
            sb.AppendLine($"{indentPrefix}{cell.Def.Description}");
            if (!jpTextOnly)
            {
                sb.AppendLine();
                sb.AppendLine($"{indentPrefix}{cell.Def.Name}");
                sb.AppendLine();
                sb.AppendLine($"{indentPrefix}Min: {cell.Def.Min}");
                sb.AppendLine($"{indentPrefix}Max: {cell.Def.Max}");
                sb.AppendLine($"{indentPrefix}Default: {cell.Def.DefaultValue}");
                sb.AppendLine($"{indentPrefix}Incrementation: {cell.Def.Increment}");
                sb.AppendLine($"{indentPrefix}Internal Value Type: {cell.Def.InternalValueType}");
                sb.AppendLine($"{indentPrefix}PARAM MAN Value Display Format: \"{cell.Def.GuiValueStringFormat}\"");
            }
        }

        private async void MenuSelectDarkSoulsDirectory_Click(object sender, RoutedEventArgs e)
        {
            await BrowseForInterrootDialog(SetLoadingMode);
        }

        private void ParamEntryList_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            var row = new ParamRow();

            row.Name = "New Entry";
            row.ReInitRawData(SelectedParam);
            row.LoadValuesFromRawData(SelectedParam);
            row.SaveDefaultValuesToRawData(SelectedParam);

            e.NewItem = row;


        }

        private async void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            PARAMDATA.LoadConfig();

            if (!string.IsNullOrWhiteSpace(PARAMDATA.Config?.InterrootPath) && CheckInterrotDirValid(PARAMDATA.Config?.InterrootPath))
            {
                await PARAMDATA.LoadParamsInOtherThread(SetLoadingMode);
            }
            else
            {
#if REMASTER
                if (MessageBox.Show("Note: ORIGINAL DARK SOULS: PREPARE TO DIE EDITION IS NOT COMPATIBLE WITH THIS VERSION OF PARAM VESSEL." +
                    "\n\n" +
                    @"Please navigate to your 'DARK SOULS REMASTERED\DarkSoulsRemastered.exe' file." +
                    "\n\nOnce the inital setup is performed, the path will be saved." +
                    "\nYou may press cancel to continue without selecting the path but the GUI will " +
                    "be blank until you go to 'File -> Select Dark Souls Directory...'",
                    "Initial Setup", MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.OK)
                {
                    await BrowseForInterrootDialog(SetLoadingMode);
                }
#else
                if (MessageBox.Show("Note: DARK SOULS REMASTERED IS NOT COMPATIBLE WITH THIS VERSION OF PARAM VESSEL." +
                    "\nYour Dark Souls: Prepare to Die Edition installation MUST be unpacked by UnpackDarkSoulsForModding by HotPocketRemix.\n\n" +
                    @"Please navigate to your 'Dark Souls Prepare to Die Edition\DATA\DARKSOULS.exe' file." +
                    "\n\nOnce the inital setup is performed, the path will be saved." +
                    "\nYou may press cancel to continue without selecting the path but the GUI will " +
                    "be blank until you go to 'File -> Select Dark Souls Directory...'",
                    "Initial Setup", MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.OK)
                {
                    await BrowseForInterrootDialog(SetLoadingMode);
                }
#endif


            }

            MainTabs.Items.Refresh();

            //RANDOM_DEBUG_TESTING();
        }

        private void CmdSave_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private async void CmdSave_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            await PARAMDATA.SaveInOtherThread(SetSavingMode);
        }

        private void SaveParamRowIndex()
        {
            var selectedParam = (MainTabs.SelectedItem as PARAMRef);

            if (selectedParam == null)
                return;

            var selectedParamEntry = ParamEntryList.SelectedItem as ParamRow;

            if (selectedParamEntry == null)
                return;

            var selIndex = selectedParam.Value.Entries.IndexOf(selectedParamEntry);

            if (selIndex >= 0)
            {
                if (PARAMDATA.Config.LastParamEntryIndices.ContainsKey(selectedParam.Key))
                {
                    PARAMDATA.Config.LastParamEntryIndices[selectedParam.Key] = selIndex;
                }
                else
                {
                    PARAMDATA.Config.LastParamEntryIndices.Add(selectedParam.Key, selIndex);
                }
            }
        }

        private void LoadParamRowIndex()
        {
            var selectedParam = (MainTabs.SelectedItem as PARAMRef);

            if (selectedParam == null)
                return;

            if (PARAMDATA.Config.LastParamEntryIndices.ContainsKey(selectedParam.Key)
                && PARAMDATA.Config.LastParamEntryIndices[selectedParam.Key] >= 0
                && PARAMDATA.Config.LastParamEntryIndices[selectedParam.Key] < ParamEntryList.Items.Count)
            {
                ParamEntryList.SelectedIndex = PARAMDATA.Config.LastParamEntryIndices[selectedParam.Key];

                ParamEntryList.ScrollIntoView(ParamEntryList.SelectedItem);
            }
        }

        private void ParamEntryList_CurrentCellChanged(object sender, EventArgs e)
        {
            SaveParamRowIndex();
        }

        private void MainTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainTabs == null)
                return;

            PARAMDATA.Config.LastParamIndex = MainTabs.SelectedIndex;

            LoadParamRowIndex();
        }

        private void ParamEntryList_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            LoadParamRowIndex();

            //SetLoadingMode(false);
        }

        private void ParamEntryList_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            SaveParamRowIndex();
        }

        private void ParamEntryList_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //TODO: Use e.Cancel, asking user if they wanna save changes and all that

            //Even if the user decides not to save the params, always save the config:
            PARAMDATA.SaveConfig();
        }

        private void MainTabs_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            SetLoadingMode(false);
            MainTabs.SelectedIndex = PARAMDATA.Config.LastParamIndex;
        }

        private async void MenuRestoreBackups_Click(object sender, RoutedEventArgs e)
        {
            await PARAMDATA.RestoreBackupsInOtherThread(SetLoadingMode);
        }

        private void ParamManStyleDataGrid_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void MenuAbout_Click(object sender, RoutedEventArgs e)
        {
            var about = new About();
            about.Owner = this;
            about.ShowDialog();
        }

        private void MenuPatchParamDefs_Click(object sender, RoutedEventArgs e)
        {
#if REMASTER
            MessageBox.Show("This action is only available on the PTDE version.", "Not Supported", MessageBoxButton.OK, MessageBoxImage.Stop);
            return;
#else
            if (MessageBox.Show("This modification will replace the Japanese display names of the variables" +
                " in the ParamDefs with their internal variable names, which are in English. " +
                "\nThese display names are used in the Dark Souls debug menu's '[PARAM MAN]' submenu. " +
                "\nNo more blindly adjusting Japanese-named variables." +
                "\n\nThis would modify the './paramdef/paramdef.paramdefbnd' file only. " +
                "\nA backup ('./paramdef/paramdef.paramdefbnd.bak') will be created before the modification is made." +
                "\n\nProceed with modification?", "Apply Modification?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (!File.Exists(PARAMDATA.Config.ParamDefBndPath + ".bak"))
                {
                    File.Copy(PARAMDATA.Config.ParamDefBndPath, PARAMDATA.Config.ParamDefBndPath + ".bak");
                }

                try
                {
                    PARAMDATA.ApplyParamDefEnglishPatch();

                    MessageBox.Show("Modification applied successfully.");
                }
                catch (Exception ex)
                {
                    if (File.Exists(PARAMDATA.Config.ParamDefBndPath + ".bak"))
                    {
                        File.Copy(PARAMDATA.Config.ParamDefBndPath + ".bak", PARAMDATA.Config.ParamDefBndPath, true);
                        MessageBox.Show("An error occurred while modifying the file (shown below). " +
                            "The file has been restored to a backup of its original state." + "\n\n" + ex.Message);
                    }
                    else
                    {
                        DataFile.SaveToFile(PARAMDATA.PARAMDEFBND, PARAMDATA.Config.ParamDefBndPath, null);

                        MessageBox.Show("An error occurred while modifying the file (shown below). " +
                            "The backup of the file's original state could not be retrieved. " +
                            "The file has been replaced with the default vanilla file." + "\n\n" + ex.Message);
                    }
                }
            }
#endif
        }

        private void MenuRandomize_Click(object sender, RoutedEventArgs e)
        {


            var rand = new Random();
            foreach (var p in PARAMDATA.Params)
            {
                var entryIdPool = new List<int>();

                for (int i = 0; i < p.Value.Entries.Count; i++)
                {
                    entryIdPool.Add(Convert.ToInt32(p.Value.Entries[i].ID));
                }

                for (int i = 0; i < p.Value.Entries.Count; i++)
                {
                    var newIdIndex = rand.Next(0, entryIdPool.Count);
                    p.Value.Entries[i].ID = entryIdPool[newIdIndex];

                    entryIdPool.RemoveAt(newIdIndex);
                }


            }

            
        }

        private void ParamRowDuplicate(/*bool newNameAndID*/)
        {
            var selectedParamRow = ParamEntryList.SelectedItem as ParamRow;
            var selectedParam = MainTabs.SelectedItem as PARAMRef;

            if (selectedParam == null || selectedParamRow == null)
                return;

            var newParamRow = new ParamRow();

            if (false /*newNameAndID*/)
            {
                //var nameEntry = new QuickTextEntry();
                //nameEntry.Title = "New Param";
                //nameEntry.TextBlockPrompt.Text = "Please input the desired"
                
            }
            else
            {
                newParamRow.ID = selectedParamRow.ID;
                newParamRow.Name = selectedParamRow.Name;
            }
            

            newParamRow.SaveDefaultValuesToRawData(selectedParam.Value);
            newParamRow.LoadValuesFromRawData(selectedParam.Value);

            foreach (var cell in newParamRow.Cells)
            {
                cell.Value = selectedParamRow[cell.Def.Name];
            }

            selectedParam.Value.Entries.Insert(selectedParam.Value.Entries.IndexOf(selectedParamRow), newParamRow);
        }

        private void ContextMenuParamRow_Duplicate_ExactCopy_Click(object sender, RoutedEventArgs e)
        {
            ParamRowDuplicate(/*false*/);
        }

        private void ParamRowCopyBytes()
        {
            var selectedParamRow = ParamEntryList.SelectedItem as ParamRow;
            var selectedParam = MainTabs.SelectedItem as PARAMRef;

            if (selectedParam == null || selectedParamRow == null)
                return;

            selectedParamRow.ReInitRawData(selectedParam.Value);
            selectedParamRow.SaveValuesToRawData(selectedParam.Value);
            var sb = new StringBuilder();
            for (int i = 0; i < selectedParamRow.RawData.Length; i++)
            {
                if (i > 0)
                    sb.Append(" ");

                sb.Append(selectedParamRow.RawData[i].ToString("X2"));
            }
            Clipboard.SetText(sb.ToString(), TextDataFormat.UnicodeText);
            selectedParamRow.ClearRawData();
        }

        private void ParamRowPasteBytes()
        {
            var selectedParamRow = ParamEntryList.SelectedItem as ParamRow;
            var selectedParam = MainTabs.SelectedItem as PARAMRef;

            if (selectedParam == null || selectedParamRow == null)
                return;

            try
            {
                var hexBytes = Clipboard.GetText(TextDataFormat.UnicodeText)
                    .Split(' ')
                    .Select(x => byte.Parse(x, System.Globalization.NumberStyles.HexNumber))
                    .ToArray();

                if (hexBytes.Length == selectedParam.Value.EntrySize)
                {
                    PARAMDATA.IsParamRowClipboardValid = true;
                }

                selectedParamRow.RawData = hexBytes;
                selectedParamRow.LoadValuesFromRawData(selectedParam.Value);

                ParamEntryList.SelectedItem = null;
                ParamEntryList.SelectedItem = selectedParamRow;
            }
            catch
            {

            }
        }

        private void ParamRowDelete()
        {
            var selectedParamRow = ParamEntryList.SelectedItem as ParamRow;
            var selectedParam = MainTabs.SelectedItem as PARAMRef;

            if (selectedParam == null || selectedParamRow == null)
                return;

            var result = MessageBox.Show($"Are you sure you wish to delete the selected entry " +
                $"({selectedParamRow.ID}: {selectedParamRow.Name})?", "Delete Entry?", 
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var indexOfSelected = selectedParam.Value.Entries.IndexOf(selectedParamRow);
                selectedParam.Value.Entries.Remove(selectedParamRow);
                if (indexOfSelected >= 0 && indexOfSelected < selectedParam.Value.Entries.Count)
                {
                    ParamEntryList.SelectedItem = selectedParam.Value.Entries[indexOfSelected];
                }
            }
        }

        private void ContextMenuParamRow_CopyAllValues_Click(object sender, RoutedEventArgs e)
        {
            ParamRowCopyBytes();
        }

        private void ContextMenuParamRow_PasteAllValues_Click(object sender, RoutedEventArgs e)
        {
            ParamRowPasteBytes();
        }

        private void ContextMenuParamRow_Opened(object sender, RoutedEventArgs e)
        {
            var selectedParamRow = ParamEntryList.SelectedItem as ParamRow;
            var selectedParam = MainTabs.SelectedItem as PARAMRef;

            if (selectedParam == null || selectedParamRow == null)
                return;

            if (Clipboard.ContainsText(TextDataFormat.UnicodeText))
            {
                try
                {
                    var hexBytes = Clipboard.GetText(TextDataFormat.UnicodeText)
                        .Split(' ')
                        .Select(x => byte.Parse(x, System.Globalization.NumberStyles.HexNumber))
                        .ToArray();

                    if (hexBytes.Length == selectedParam.Value.EntrySize)
                    {
                        PARAMDATA.IsParamRowClipboardValid = true;
                    }
                    else
                    {
                        PARAMDATA.IsParamRowClipboardValid = false;
                    }

                    
                }
                catch
                {
                    PARAMDATA.IsParamRowClipboardValid = false;
                }
                
            }
        }

        private void ParamEntryList_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {

                }
                else
                {
                    if (e.Key == Key.D)
                    {
                        ParamRowDuplicate();
                    }
                    else if (e.Key == Key.C)
                    {
                        ParamRowCopyBytes();
                    }
                    else if (e.Key == Key.V)
                    {
                        ParamRowPasteBytes();
                    }
                }
                
            }
            else if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                if (e.Key == Key.Delete)
                {
                    ParamRowDelete();
                }
            }

        }

        private void ContextMenuParamRow_Delete_Click(object sender, RoutedEventArgs e)
        {
            ParamRowDelete();
        }
    }
}
