﻿using HKLib.hk2018.hkAsyncThreadPool;
using HKLib.hk2018.hkHashMapDetail;
using SoulsFormats;
using StudioCore.Editor;
using StudioCore.Editors.HavokEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static SoulsFormats.DRB;
using static StudioCore.Editors.TimeActEditor.AnimationBank;
using static StudioCore.Editors.TimeActEditor.TimeActUtils;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActSelectionHandler
{
    private ActionManager EditorActionManager;
    private TimeActEditorScreen Screen;

    public HavokContainerInfo LoadedHavokContainer;

    public ContainerFileInfo ContainerInfo;
    public BinderInfo ContainerBinder;
    public string ContainerKey;
    public int ContainerIndex = -1;

    public TAE CurrentTimeAct;
    public int CurrentTimeActKey;

    public TAE.Animation CurrentTimeActAnimation;
    public TemporaryAnimHeader CurrentTemporaryAnimHeader;
    public int CurrentTimeActAnimationIndex = -1;

    public TAE.Event CurrentTimeActEvent;
    public int CurrentTimeActEventIndex = -1;

    public string CurrentTimeActEventProperty;
    public int CurrentTimeActEventPropertyIndex = -1;

    public TimeActMultiselect TimeActMultiselect;

    public TimeActContextMenu ContextMenu;

    public TemplateType CurrentTimeActType = TemplateType.Character;

    public SelectionContext CurrentSelectionContext = SelectionContext.None;
    public FileContainerType CurrentFileContainerType = FileContainerType.None;

    public enum SelectionContext
    {
        None,
        File,
        TimeAct,
        Animation,
        Event,
        Property
    }

    public enum FileContainerType
    {
        None,
        Character,
        Object
    }

    public TimeActSelectionHandler(ActionManager editorActionManager, TimeActEditorScreen screen)
    {
        EditorActionManager = editorActionManager;
        Screen = screen;

        TimeActMultiselect = new(Screen);

        ContextMenu = new(screen, this);
    }

    public void OnProjectChanged()
    {
        ResetSelection();
    }

    public void ResetSelection()
    {
        ContainerIndex = -1;
        ContainerKey = null;
        ContainerInfo = null;
        ContainerBinder = null;

        CurrentTimeActKey = -1;
        CurrentTimeAct = null;

        CurrentTimeActAnimation = null;
        CurrentTimeActAnimationIndex = -1;
        CurrentTemporaryAnimHeader = null;

        CurrentTimeActEvent = null;
        CurrentTimeActEventIndex = -1;

        CurrentTimeActEventProperty = null;
        CurrentTimeActEventPropertyIndex = -1;

        TimeActMultiselect.Reset(false, false, true);
    }

    public void FileContainerChange(ContainerFileInfo info, BinderInfo binderInfo, int index, FileContainerType containerType, bool changeContext = true)
    {
        CurrentFileContainerType = containerType;

        if (changeContext)
            CurrentSelectionContext = SelectionContext.File;

        ContainerIndex = index;
        ContainerKey = info.Name;
        ContainerInfo = info;
        ContainerBinder = binderInfo;

        CurrentTimeActKey = -1;
        CurrentTimeAct = null;

        CurrentTimeActAnimation = null;
        CurrentTimeActAnimationIndex = -1;
        CurrentTemporaryAnimHeader = null;

        CurrentTimeActEvent = null;
        CurrentTimeActEventIndex = -1;

        CurrentTimeActEventProperty = null;
        CurrentTimeActEventPropertyIndex = -1;

        TimeActMultiselect.Reset(true, true, true);

        // Auto-Select first TimeAct if not empty
        if(ContainerInfo.InternalFiles.Count > 0)
        {
            for(int i = 0; i < ContainerInfo.InternalFiles.Count; i++)
            {
                var timeAct = ContainerInfo.InternalFiles[i].TAE;
                TimeActChange(timeAct, i, false);
                break;
            }
        }
    }

    public void ResetOnTimeActChange()
    {
        CurrentTimeActKey = -1;
        CurrentTimeAct = null;

        CurrentTimeActAnimation = null;
        CurrentTimeActAnimationIndex = -1;
        CurrentTemporaryAnimHeader = null;

        CurrentTimeActEvent = null;
        CurrentTimeActEventIndex = -1;

        CurrentTimeActEventProperty = null;
        CurrentTimeActEventPropertyIndex = -1;

        TimeActMultiselect.Reset(true, true, true);
    }

    public void TimeActChange(TAE entry, int index, bool changeContext = true)
    {
        if(changeContext)
            CurrentSelectionContext = SelectionContext.TimeAct;

        TimeActMultiselect.TimeActSelection(CurrentTimeActKey, index);

        CurrentTimeActKey = index;
        CurrentTimeAct = entry;

        CurrentTimeActAnimation = null;
        CurrentTemporaryAnimHeader = null;
        CurrentTimeActAnimationIndex = -1;

        CurrentTimeActEvent = null;
        CurrentTimeActEventIndex = -1;

        CurrentTimeActEventProperty = null;
        CurrentTimeActEventPropertyIndex = -1;

        TimeActMultiselect.Reset(false, true, true);

        TimeActUtils.ApplyTemplate(CurrentTimeAct, CurrentTimeActType);

        // Auto-Select first Animation if not empty
        if (CurrentTimeAct.Animations.Count > 0)
        {
            for (int i = 0; i < CurrentTimeAct.Animations.Count; i++)
            {
                var anim = CurrentTimeAct.Animations[i];
                TimeActAnimationChange(anim, i, false);
                break;
            }
        }
    }

    public void ResetOnTimeActAnimationChange()
    {
        CurrentTimeActAnimation = null;
        CurrentTimeActAnimationIndex = -1;
        CurrentTemporaryAnimHeader = null;

        CurrentTimeActEvent = null;
        CurrentTimeActEventIndex = -1;

        CurrentTimeActEventProperty = null;
        CurrentTimeActEventPropertyIndex = -1;

        TimeActMultiselect.Reset(false, true, true);
    }

    public void TimeActAnimationChange(TAE.Animation entry, int index, bool changeContext = true)
    {
        if (changeContext)
            CurrentSelectionContext = SelectionContext.Animation;

        TimeActMultiselect.AnimationSelection(CurrentTimeActAnimationIndex, index);

        CurrentTimeActAnimation = entry;
        CurrentTimeActAnimationIndex = index;
        CurrentTemporaryAnimHeader = null;

        CurrentTimeActEvent = null;
        CurrentTimeActEventIndex = -1;

        CurrentTimeActEventProperty = null;
        CurrentTimeActEventPropertyIndex = -1;

        // If a filter is active, auto-select first result (if any), since this is more user-friendly
        if(TimeActFilters._timeActEventFilterString != "")
        {
            Screen.SelectFirstEvent = true;
        }

        TimeActMultiselect.Reset(false, false, true);

        // Auto-Select first Event if not empty
        if (CurrentTimeActAnimation.Events.Count > 0)
        {
            for (int i = 0; i < CurrentTimeActAnimation.Events.Count; i++)
            {
                var evt = CurrentTimeActAnimation.Events[i];
                TimeActEventChange(evt, i, false);
                break;
            }
        }
    }

    public void ResetOnTimeActEventChange()
    {
        CurrentTimeActEvent = null;
        CurrentTimeActEventIndex = -1;

        CurrentTimeActEventProperty = null;
        CurrentTimeActEventPropertyIndex = -1;

        TimeActMultiselect.Reset(false, false, true);
    }

    public void TimeActEventChange(TAE.Event entry, int index, bool changeContext = true)
    {
        if (changeContext)
            CurrentSelectionContext = SelectionContext.Event;

        TimeActMultiselect.EventSelection(CurrentTimeActEventIndex, index);

        CurrentTimeActEvent = entry;
        CurrentTimeActEventIndex = index;

        CurrentTimeActEventProperty = null;
        CurrentTimeActEventPropertyIndex = -1;
    }

    public void TimeActEventPropertyChange(string entry, int index)
    {
        CurrentSelectionContext = SelectionContext.Property;

        CurrentTimeActEventProperty = entry;
        CurrentTimeActEventPropertyIndex = index;
    }

    public bool HasSelectedFileContainer()
    {
        return ContainerInfo != null;
    }

    public bool HasSelectedTimeAct()
    {
        return CurrentTimeAct != null;
    }

    public bool HasSelectedTimeActAnimation()
    {
        return CurrentTimeActAnimation != null;
    }

    public bool HasSelectedTimeActEvent()
    {
        return CurrentTimeActEvent != null;
    }
}
