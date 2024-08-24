﻿using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Veldrid;

namespace StudioCore;

[JsonSourceGenerationOptions(WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata, IncludeFields = true)]
[JsonSerializable(typeof(KeyBindings.Bindings))]
[JsonSerializable(typeof(KeyBind))]
internal partial class KeybindingsSerializerContext : JsonSerializerContext
{
}

public enum KeybindCategory
{
    Core,
    Window,
    MapEditor,
    ModelEditor,
    ParamEditor,
    TextEditor,
    TimeActEditor,
    Viewport,
    TextureViewer
}

public class KeyBind
{
    public bool Alt_Pressed;
    public bool Ctrl_Pressed;
    public Key PrimaryKey;
    public bool Shift_Pressed;
    public bool FixedKey;

    public string PresentationName;
    public string Description;

    [JsonConstructor]
    public KeyBind()
    {
        PresentationName = "";
        Description = "";
    }

    public KeyBind(string name, string description, Key primaryKey = Key.Unknown, bool ctrlKey = false, bool altKey = false, bool shiftKey = false, bool fixedKey = false)
    {
        PresentationName = name;
        Description = description;

        PrimaryKey = primaryKey;
        Ctrl_Pressed = ctrlKey;
        Alt_Pressed = altKey;
        Shift_Pressed = shiftKey;
        FixedKey = fixedKey;
    }

    [JsonIgnore]
    public string HintText
    {
        get
        {
            if (PrimaryKey == Key.Unknown)
            {
                return "";
            }

            var str = "";
            if (Ctrl_Pressed)
            {
                str += "Ctrl+";
            }

            if (Alt_Pressed)
            {
                str += "Alt+";
            }

            if (Shift_Pressed)
            {
                str += "Shift+";
            }

            str += PrimaryKey.ToString();
            return str;
        }
    }
}

public class KeyBindings
{
    public static Bindings Current { get; set; }
    public static Bindings Default { get; set; } = new();

    public static void ResetKeyBinds()
    {
        Current = new Bindings();
    }

    public class Bindings
    {
        //-----------------------------
        // Core
        //-----------------------------
        // Core
        public KeyBind CORE_CreateNewEntry = new(
            "Create New Entry", 
            "Creates a new default entry based on the current selection context.", 
            Key.Insert);

        public KeyBind CORE_DeleteSelectedEntry = new(
            "Delete Selected Entry", 
            "Deletes the selected entry or entries based on the current selection context.", 
            Key.Delete);

        public KeyBind CORE_DuplicateSelectedEntry = new(
            "Duplicate",
            "Duplicates the selected entry or entries based on the current selection context.",
            Key.D, 
            true);

        public KeyBind CORE_RedoAction = new(
            "Redo", 
            "Re-executes a previously un-done action.", 
            Key.Y, 
            true);

        public KeyBind CORE_UndoAction = new(
            "Undo",
            "Undoes a previously executed action.",
            Key.Z,
            true);

        public KeyBind CORE_SaveAll = new(
            "Save All", 
            "Saves all modified files within the focused editor.",
            Key.Unknown);

        public KeyBind CORE_Save = new(
            "Save", 
            "Save the current file-level selection within the focused editor.", 
            Key.S, 
            true);

        // Windows
        public KeyBind CORE_ConfigurationWindow = new(
            "Configuration Window", 
            "Toggles the visibility of the Configuration window.",
            Key.F2);

        public KeyBind CORE_HelpWindow = new(
            "Help Window",
            "Toggles the visibility of the Help window.",
            Key.F3);

        public KeyBind CORE_KeybindingWindow = new(
            "Keybinds Window",
            "Toggles the visibility of the Keybinds window.",
            Key.F5);

        //-----------------------------
        // Viewport
        //-----------------------------
        // Core
        public KeyBind VIEWPORT_CameraForward = new(
            "Move Forward",
            "Moves the camera forward.",
            Key.W);

        public KeyBind VIEWPORT_CameraBack = new(
            "Move Back",
            "Moves the camera backwards.", 
            Key.S);

        public KeyBind VIEWPORT_CameraUp = new(
            "Move Up",
            "Moves the camera upwards.",
            Key.E);

        public KeyBind VIEWPORT_CameraDown = new(
            "Move Down",
            "Moves the camera downwards.", 
            Key.Q);

        public KeyBind VIEWPORT_CameraLeft = new(
            "Move Left",
            "Moves the camera leftwards.", 
            Key.A);

        public KeyBind VIEWPORT_CameraRight = new(
            "Move Right",
            "Moves the camera rightwards.",
            Key.D);

        public KeyBind VIEWPORT_CameraReset = new(
            "Reset Position", 
            "Resets the camera's position to (0,0,0)", 
            Key.R);

        // Gizmos
        public KeyBind VIEWPORT_GizmoRotationMode = new(
            "Cycle Gizmo Rotation Mode", 
            "Cycles through the gizmo rotation modes.", 
            Key.E);

        public KeyBind VIEWPORT_GizmoOriginMode = new(
            "Cycle Gizmo Origin Mode",
            "Cycles through the gizmo origin modes.",
            Key.Home);

        public KeyBind VIEWPORT_GizmoSpaceMode = new(
            "Cycle Gizmo Space Mode",
            "Cycles through the gizmo space modes.",
            Key.Unknown);

        public KeyBind VIEWPORT_GizmoTranslationMode = new(
            "Cycle Gizmo Translation Mode",
            "Cycles through the gizmo translation modes.", 
            Key.W);

        // Grid
        public KeyBind VIEWPORT_LowerGrid = new(
            "Lower Grid",
            "Lowers the viewport grid height by the specified unit increment.", 
            Key.Q, 
            true);

        public KeyBind VIEWPORT_RaiseGrid = new(
            "Raise Grid",
            "Raises the viewport grid height by the specified unit increment.",
            Key.E, 
            true);

        public KeyBind VIEWPORT_SetGridToSelectionHeight = new(
            "Move Grid to Selection Height",
            "Set the viewport grid height to the height of the current selection.",
            Key.K, 
            true);

        // Selection
        public KeyBind VIEWPORT_RenderOutline = new(
            "Toggle Selection Outline", 
            "Toggles the appearance of the selection outline.",
            Key.Unknown);

        //-----------------------------
        // Map Editor
        //-----------------------------
        // Core
        public KeyBind MAP_GoToInList = new(
            "Go to in List",
            "Go to the selection within the Map Object List.",
            Key.G);

        public KeyBind MAP_MoveToCamera = new(
            "Move Selection to Camera",
            "Moves the current selection to the camera's position.",
            Key.X);

        public KeyBind MAP_FrameSelection = new(
            "Frame Selection",
            "Frames the current selection within the viewport.",
            Key.F);

        public KeyBind MAP_RotateSelectionXAxis = new(
            "Rotate Selection on X-axis", 
            "Rotates the current selection on the X-axis by the specified increment.", 
            Key.R,
            true);

        public KeyBind MAP_RotateSelectionYAxis = new(
            "Rotate Selection on Y-axis",
            "Rotates the current selection on the Y-axis by the specified increment.",
            Key.Unknown);

        public KeyBind MAP_PivotSelectionYAxis = new(
            "Pivot Selection on Y-axis",
            "Pivots the current selection on the Y-axis by the specified increment.",
            Key.R,
            false,
            false,
            true);

        public KeyBind MAP_RotateFixedAngle = new(
            "Rotate to Fixed Increment for Selection",
            "Increment the rotation of the current selection to the fixed angle defined in the tool window.",
            Key.Unknown);

        public KeyBind MAP_ResetRotation = new(
            "Reset Rotation for Selection",
            "Resets the rotation of the current selection to (0,0,0).",
            Key.R,
            false,
            true);

        public KeyBind MAP_FlipSelectionVisibility = new(
            "Flip Editor Visibility of Selection", 
            "Flip the editor visibility state for the current selection.", 
            Key.H, 
            true);

        public KeyBind MAP_FlipAllVisibility = new(
            "Flip Editor Visibility of All",
            "Flip the editor visibility state for all map objects.",
            Key.B, 
            true);

        public KeyBind MAP_EnableSelectionVisibility = new(
            "Enable Editor Visibility of Selection",
            "Enable the editor visibility for the current selection.",
            Key.H,
            true, 
            true);

        public KeyBind MAP_EnableAllVisibility = new(
            "Enable Editor Visibility of All",
            "Enable the editor visibility for all map objects.",
            Key.B, 
            true, 
            true);

        public KeyBind MAP_DisableSelectionVisibility = new(
            "Disable Editor Visibility of Selection",
            "Disable the editor visibility for the current selection.",
            Key.H, 
            true, 
            true, 
            true);

        public KeyBind MAP_DisableAllVisibility = new(
            "Disable Editor Visibility of All",
            "Disable the editor visibility for all map objects.",
            Key.B, 
            true, 
            true, 
            true);

        public KeyBind MAP_MakeDummyObject = new(
            "Make Selection into Dummy Object", 
            "Changes the current selection into the equivalent Dummy map object type (if possible).",
            Key.Comma, 
            false, 
            false, 
            true);

        public KeyBind MAP_MakeNormalObject = new(
            "Make Selection into Normal Object",
            "Changes the current selection (if Dummy objects) into the equivalent normal map object type.",
            Key.Period, 
            false, 
            false, 
            true);

        public KeyBind MAP_DisableGamePresence = new(
            "Disable Game Presence of Selection",
            "Changes the current selection GameEditionDisable to 0, hiding it in-game.",
            Key.Unknown);

        public KeyBind MAP_EnableGamePresence = new(
            "Enable Game Presence of Selection",
            "Changes the current selection GameEditionDisable to 1, display it in-game.",
            Key.Unknown);

        public KeyBind MAP_ScrambleSelection = new(
            "Scramble Selection", 
            "Scrambles the position, rotation and scale (depending on Scramble tool settings) of the current selection.", 
            Key.S, 
            false, 
            true);

        public KeyBind MAP_ReplicateSelection = new(
            "Replicate Selection", 
            "Replicates the current selection (based on the Replicate tool settings).", 
            Key.R, 
            false, 
            true);

        public KeyBind MAP_SetSelectionToGrid = new(
            "Set Selection Height to Grid Height", 
            "Moves the current selection's height to the height of the viewport grid.", 
            Key.G, 
            false, 
            true);

        public KeyBind MAP_CreateMapObject = new(
            "Create Map Object", 
            "Create a new map object of the selected type with default values.", 
            Key.C, 
            false, 
            true);

        public KeyBind MAP_TogglePatrolRouteRendering = new(
            "Toggle Patrol Route Connections", 
            "Toggles the rendering of patrol route connections.", 
            Key.P, 
            true);

        public KeyBind MAP_DuplicateToMap = new(
            "Duplicate Selection to Map", 
            "Duplicates the current selection into the targeted map.", 
            Key.D, 
            false, 
            false, 
            true);

        // Render Groups
        public KeyBind MAP_GetDisplayGroup = new(
            "View Display Group", 
            "Display the display group for the current selection.", 
            Key.G, 
            true);

        public KeyBind MAP_GetDrawGroup = new(
            "View Draw Group",
            "Display the draw group for the current selection.",
            Key.Unknown);

        public KeyBind MAP_SetDisplayGroup = new(
            "Set Display Group", 
            "Set the display group (as appears in the Render Groups tab) to the current selection.",
            Key.Unknown);

        public KeyBind MAP_SetDrawGroup = new(
            "Render Group: Give Draw Group",
            "Set the draw group (as appears in the Render Groups tab) to the current selection.",
            Key.Unknown);

        public KeyBind MAP_HideAllDisplayGroups = new(
            "Hide All Display Groups", 
            "Set the current selection display groups to 0.",
            Key.Unknown);

        public KeyBind MAP_ShowAllDisplayGroups = new(
            "Show All Display Groups",
            "Set the current selection display groups to 0xFFFFFFFF.",
            Key.R, 
            true);

        public KeyBind MAP_SelectDisplayGroupHighlights = new(
            "Select Display Group Highlights", 
            "Select the objects that match the current display groups.",
            Key.Unknown);

        // Selection Group
        public KeyBind MAP_CreateSelectionGroup = new(
            "Create Selection Group", 
            "Creates a new selection group from current selection.", 
            Key.L, 
            false, 
            true);

        public KeyBind MAP_SelectionGroup_0 = new(
            "Select Selection Group 0", 
            "Select the contents of Selection Group 0 (if defined).", 
            Key.Keypad0, 
            false,
            false);

        public KeyBind MAP_SelectionGroup_1 = new(
            "Select Selection Group 1",
            "Select the contents of Selection Group 1 (if defined).",
            Key.Keypad1, 
            false, 
            false);

        public KeyBind MAP_SelectionGroup_2 = new(
            "Select Selection Group 2",
            "Select the contents of Selection Group 2 (if defined).",
            Key.Keypad2, 
            false, 
            false);

        public KeyBind MAP_SelectionGroup_3 = new(
            "Select Selection Group 3",
            "Select the contents of Selection Group 3 (if defined).",
            Key.Keypad3, 
            false,
            false);


        public KeyBind MAP_SelectionGroup4 = new(
            "Select Selection Group 4",
            "Select the contents of Selection Group 4 (if defined).",
            Key.Keypad4,
            false,
            false);


        public KeyBind MAP_SelectionGroup5 = new(
            "Select Selection Group 5",
            "Select the contents of Selection Group 5 (if defined).",
            Key.Keypad5,
            false,
            false);


        public KeyBind MAP_SelectionGroup6 = new(
            "Select Selection Group 6",
            "Select the contents of Selection Group 6 (if defined).",
            Key.Keypad6,
            false,
            false);

        public KeyBind MAP_SelectionGroup7 = new(
            "Select Selection Group 7",
            "Select the contents of Selection Group 7 (if defined).",
            Key.Keypad7,
            false,
            false);


        public KeyBind MAP_SelectionGroup8 = new(
            "Select Selection Group 8",
            "Select the contents of Selection Group 8 (if defined).",
            Key.Keypad8,
            false,
            false);


        public KeyBind MAP_SelectionGroup9 = new(
            "Select Selection Group 9",
            "Select the contents of Selection Group 9 (if defined).",
            Key.Keypad9,
            false,
            false);

        public KeyBind MAP_SelectionGroup10 = new(
            "Select Selection Group 10",
            "Select the contents of Selection Group 10 (if defined).",
            Key.KeypadAdd, 
            false, 
            false);

        // Order
        public KeyBind MAP_MoveObjectUp = new(
            "Move Map Object Up in List", 
            "Moves the selected map object up on the Map Object List order.", 
            Key.U, 
            true, 
            false);

        public KeyBind MAP_MoveObjectDown = new(
            "Move Map Object Down in List",
            "Moves the selected map object down on the Map Object List order.",
            Key.J,
            true, 
            false);

        public KeyBind MAP_MoveObjectTop = new(
            "Move Map Object to Top in List",
            "Moves the selected map object to the top of the Map Object List order.",
            Key.U, 
            true, 
            true);

        public KeyBind MAP_MoveObjectBottom = new(
            "Move Map Object to Bottom in List",
            "Moves the selected map object to the bottom of the Map Object List order.",
            Key.J, 
            true, 
            true);

        // World Map
        public KeyBind MAP_ToggleERMapVanilla = new(
            "Toggle Lands Between Map", 
            "Toggles the visibility of the Lands Between map.",
            Key.M, 
            true, 
            false, 
            false);

        public KeyBind MAP_ToggleERMapSOTE = new(
            "Toggle Land of Shadow Map", 
            "Toggles the visibility of the Land of Shadow map",
            Key.M, 
            true, 
            true, 
            false);

        public KeyBind MAP_DragWorldMap = new(
            "Drag World Map", 
            "Held to drag around the world map.", 
            Key.C, 
            false, 
            false, 
            false);

        //-----------------------------
        // Model Editor
        //-----------------------------
        // Core
        public KeyBind MODEL_ToggleVisibility = new(
            "Toggle Section Visibility", 
            "Applies visibility change to all members of the section when clicking the visibility eye icon.", 
            Key.A);

        public KeyBind MODEL_Multiselect = new(
            "Multi-Select Row", 
            "When held, multiple rows may be selected.", 
            Key.Z);

        public KeyBind MODEL_MultiselectRange = new(
            "Multi-Select Row Range", 
            "When held, the next row selected will be considered the 'start', and the next row after that the 'end'. All rows between them will be selected.",
            Key.LShift, 
            false, 
            false, 
            false, 
            true);

        public KeyBind MODEL_ExportModel = new(
            "Export Model", 
            "Export the currently loaded model as a .DAE file.", 
            Key.K, 
            true);

        //-----------------------------
        // Param Editor
        //-----------------------------
        // Core
        public KeyBind PARAM_SelectAll = new(
            "Select All",
            "Select all rows.",
            Key.A,
            true);

        public KeyBind PARAM_GoToSelectedRow = new(
            "Go to Selected Row",
            "Change the list view to the currently selected row.",
            Key.G);

        public KeyBind PARAM_GoToRowID = new(
            "Go to Row ID",
            "Trigger the Row ID search prompt.",
            Key.G,
            true);

        public KeyBind PARAM_SortRows = new(
            "Sort Rows",
            "Sort the rows of the currently selected param",
            Key.Unknown);

        public KeyBind PARAM_CopyToClipboard = new(
            "Copy Selection to Clipboard", 
            "Copies the current param row to the clipboard.",
            Key.C, 
            true);

        public KeyBind PARAM_PasteClipboard = new(
            "Paste Clipboard",
            "Paste the current param row in the clipboard.",
            Key.V,
            true);

        public KeyBind PARAM_SearchParam = new(
            "Focus Param Search",
            "Moves focus to the param search input.",
            Key.P,
            true);

        public KeyBind PARAM_SearchRow = new(
            "Focus Row Search",
            "Moves focus to the row search input.",
            Key.F,
            true);

        public KeyBind PARAM_SearchField = new(
            "Focus Field Search",
            "Moves focus to the field search input.",
            Key.N,
            true);

        // Mass Edit
        public KeyBind PARAM_ViewMassEdit = new(
            "View Mass Edit",
            "Trigger the Mass Edit prompt.",
            Key.Unknown);

        public KeyBind PARAM_ExecuteMassEdit = new(
            "Execute Mass Edit",
            "Execute the current Mass Edit input (if any).",
            Key.Q,
            true);

        // CSV
        public KeyBind PARAM_ImportCSV = new(
            "Import CSV",
            "Trigger the CSV Import prompt.",
            Key.Unknown);

        public KeyBind PARAM_ExportCSV = new(
            "Export CSV", 
            "Trigger the CSV Export prompt.",
            Key.Unknown);

        // Row Namer
        public KeyBind PARAM_ApplyRowNamer = new(
            "Apply Row Namer (Flat)",
            "Apply the Row Namer to the current row selection, with the Flat configuration.",
            Key.I,
            true);

        // Param Reloader
        public KeyBind PARAM_ReloadParam = new(
            "Reload Current Param", 
            "Reloads the rows of the current Param selection in-game.", 
            Key.F5);

        public KeyBind PARAM_ReloadAllParams = new(
            "Reload All Params",
            "Reloads the rows of all Params in-game.",
            Key.F5, 
            false, 
            false, 
            true);

        // Pin Groups
        public KeyBind PARAM_CreateParamPinGroup = new(
            "Create Param Pin Group",
            "Create a new Param pin group from the currently pinned params.",
            Key.Unknown);

        public KeyBind PARAM_CreateRowPinGroup = new(
            "Create Row Pin Group",
            "Create a new Row pin group from the currently pinned rows.",
            Key.Unknown);

        public KeyBind PARAM_CreateFieldPinGroup = new(
            "Create Field Pin Group",
            "Create a new Field pin group from the currently pinned fields.",
            Key.Unknown,
            true);

        public KeyBind PARAM_ClearCurrentPinnedParams = new(
            "Clear Pinned Params",
            "Clear currently pinned params.",
            Key.Unknown);

        public KeyBind PARAM_ClearCurrentPinnedRows = new(
            "Clear Pinned Rows",
            "Clear currently pinned Rows.",
            Key.Unknown);

        public KeyBind PARAM_ClearCurrentPinnedFields = new(
            "Clear Pinned Fields",
            "Clear currently pinned Fields.",
            Key.Unknown);

        public KeyBind PARAM_OnlyShowPinnedParams = new(
            "Show Pinned Params Only",
            "Toggle the setting to show only pinned params in the param list.",
            Key.Unknown);

        public KeyBind PARAM_OnlyShowPinnedRows = new(
            "Show Pinned Rows Only",
            "Toggle the setting to show only pinned rows in the param list.",
            Key.Unknown);

        public KeyBind PARAM_OnlyShowPinnedFields = new(
            "Show Pinned Fields Only",
            "Toggle the setting to show only pinned fields in the param list.",
            Key.Unknown);

        //-----------------------------
        // Text Editor
        //-----------------------------
        // Core
        public KeyBind TEXT_FocusSearch = new(
            "Focus Text Search",
            "Moves focus to the Text search input.",
            Key.F,
            true);

        public KeyBind TEXT_SyncDescriptions = new(
            "Sync Descriptions", 
            "Sync the descriptions of the selected text entries.", 
            Key.K, 
            true);

        //-----------------------------
        // GPARAM Editor
        //-----------------------------
        // Core
        public KeyBind GPARAM_ExecuteQuickEdit = new(
            "Execute Quick Edit Commands",
            "Execute the current quick edit commands.",
            Key.E,
            true);

        public KeyBind GPARAM_GenerateQuickEdit = new(
            "Generate Quick Edit Commands",
            "Generate quick edit commands from current selection.",
            Key.K,
            true);

        public KeyBind GPARAM_ClearQuickEdit = new(
            "Clear Quick Edit Commands",
            "Clear current quick edit commands.",
            Key.L,
            true);

        //-----------------------------
        // Time Act Editor
        //-----------------------------
        // Core
        public KeyBind TIMEACT_Multiselect = new(
            "Multi-Select Row",
            "When held, multiple rows may be selected.",
            Key.Z);

        public KeyBind TIMEACT_MultiselectRange = new(
            "Multi-Select Row Range", 
            "When held, the next row selected will be considered the 'start', and the next row after that the 'end'. All rows between them will be selected.",
            Key.LShift, 
            false, 
            false, 
            false, 
            true);

        //-----------------------------
        // Texture Viewer
        //-----------------------------
        // Core
        public KeyBind TEXTURE_ExportTexture = new(
            "Export Texture", 
            "Export the currently viewed texture.", 
            Key.X, 
            true);

        public KeyBind TEXTURE_ZoomMode = new(
            "Zoom Mode", 
            "When held, the texture may be zoomed in/out with the mouse wheel.", 
            Key.LControl, 
            false, 
            false, 
            false, 
            true);

        public KeyBind TEXTURE_ResetZoomLevel = new(
            "Reset Zoom Level", 
            "Resets the zoom level to default.", 
            Key.R);

        //-----------------------------
        // Misc
        //-----------------------------
#pragma warning disable IDE0051
        // JsonExtensionData stores info in config file not present in class in order to retain settings between versions.
        [JsonExtensionData] internal IDictionary<string, JsonElement> AdditionalData { get; set; }
#pragma warning restore IDE0051
    }
}
