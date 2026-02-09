import { useEffect, useMemo, useState } from 'react';
import type { CoreSettingsBlock, ModuleRow, NotificationType } from '../../types';
import { getModules, createModule, UpsertModuleRequest, uploadIcon, uploadHero, exportDevCore, getIconUrl, getHeroUrl } from './api';
import { generateModuleId } from '../../core/utils/generateModuleId';

export function useModules(apiBase: string, token: string | null, setGlobalStatus: (s: string) => void, isAdvanced: boolean) {
  const [modules, setModules] = useState<ModuleRow[]>([]);
  const [loading, setLoading] = useState(false);
  const [search, setSearch] = useState('');
  const [categoryFilter, setCategoryFilter] = useState('All');
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());
  const [iconFile, setIconFile] = useState<File | null>(null);
  const [heroFile, setHeroFile] = useState<File | null>(null);
  const [iconPreviewUrl, setIconPreviewUrl] = useState<string | null>(null);
  const [heroPreviewUrl, setHeroPreviewUrl] = useState<string | null>(null);
  const [localHeroPreview, setLocalHeroPreview] = useState<string | null>(null);

  const loadModules = async (silent = false) => {
    if (!token) {
      if (!silent) setGlobalStatus('Login first.');
      return;
    }
    try {
      setLoading(true);
      if (!silent) setGlobalStatus('Loading modules...');
      const data = await getModules(apiBase, token);
      setModules(data);
      if (!silent) setGlobalStatus(`Loaded ${data.length} modules.`);
    } catch (err: any) {
      if (!silent) setGlobalStatus(err?.message ?? 'Error loading modules.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (token) {
      loadModules();
    }
  }, [token, apiBase]);

  const filteredModules = useMemo(() => {
    const term = search.trim().toLowerCase();
    return modules.filter((m) => {
      const matchesTerm =
        !term ||
        m.displayName.toLowerCase().includes(term) ||
        m.moduleId.toLowerCase().includes(term);
      const matchesCat = categoryFilter === 'All' || m.category === categoryFilter;
      return matchesTerm && matchesCat;
    });
  }, [modules, search, categoryFilter]);

  const selectedModuleId = selectedIds.size === 1 ? Array.from(selectedIds)[0] : null;

  const createEmptyFormState = (type: NotificationType) => ({
    id: '',
    displayName: '',
    moduleId: generateModuleId(''),
    title: '',
    message: '',
    category: 'GeneralInfo',
    type,
    linkUrl: 'https://',
    schedule: '',
    expires: '',
    reminderHours: '1',
    icon: 'info.ico',
    customIcon: 'info.ico',
    soundEnabled: true,
    conditionalScript: '',
    conditionalInterval: '60',
    dynamicScript: '',
    dynamicMaxLength: '160',
    dynamicTrimWhitespace: true,
    dynamicFailIfEmpty: true,
    dynamicFallbackMessage: '',
    coreEnabled: true,
    coreAutoClear: true,
    corePolling: '300',
    coreHeartbeat: '15',
    coreSound: true,
    coreExitVisible: false,
    coreStartStopVisible: false
  });

  const [formType, setFormType] = useState<NotificationType>('Standard');
  const [formState, setFormState] = useState(createEmptyFormState('Standard'));
  const [removing, setRemoving] = useState(false);

  // Keep the form state's type in sync with the active tab
  useEffect(() => {
    setFormState((prev) => ({ ...prev, type: formType }));
  }, [formType]);

  const resetForm = (keepType?: NotificationType) => {
    const nextType = keepType ?? formType;
    setFormType(nextType);
    setFormState(createEmptyFormState(nextType));
    setSelectedIds(new Set());
    setIconFile(null);
    setHeroFile(null);
    setIconPreviewUrl(null);
    setHeroPreviewUrl(null);
    setLocalHeroPreview(null);
  };

  const saveModule = async () => {
    if (!token) {
      setGlobalStatus('Login first.');
      return;
    }
    if (!isAdvanced && formState.type === 'CoreSettings') {
      setGlobalStatus('Core Settings can only be created by admin.');
      return;
    }
    const { displayName, moduleId, title, message, category, type, linkUrl, schedule, expires, reminderHours,
      icon, soundEnabled, conditionalScript, conditionalInterval, dynamicScript,
      dynamicMaxLength, dynamicTrimWhitespace, dynamicFailIfEmpty, dynamicFallbackMessage,
      coreEnabled, coreAutoClear, corePolling, coreHeartbeat, coreSound, coreExitVisible, coreStartStopVisible } = formState;

    if (!displayName.trim()) {
      setGlobalStatus('Name is required.');
      return;
    }
    if (!moduleId.trim()) {
      setGlobalStatus('Module ID is required.');
      return;
    }
    if (type !== 'CoreSettings') {
      if (!title.trim()) {
        setGlobalStatus('Title is required.');
        return;
      }
      if ((type === 'Standard' || type === 'Conditional') && !message.trim()) {
        setGlobalStatus('Message is required for this module type.');
        return;
      }
      if (type === 'Dynamic' && !dynamicScript.trim()) {
        setGlobalStatus('Dynamic script is required.');
        return;
      }
      if (type === 'Hero' && !heroFile) {
        setGlobalStatus('Select a hero PNG (2:1 ratio) before saving.');
        return;
      }
    }

    const payload: UpsertModuleRequest = {
      displayName,
      moduleId,
      type,
      category,
      title: type === 'CoreSettings' ? '' : title,
      message: type === 'CoreSettings' ? undefined : message,
      linkUrl: type === 'CoreSettings' ? undefined : linkUrl,
      scheduleUtc: type === 'CoreSettings' ? null : (schedule ? new Date(schedule).toISOString() : null),
      expiresUtc: type === 'CoreSettings' ? null : (expires ? new Date(expires).toISOString() : null),
      reminderHours: type === 'CoreSettings' ? undefined : reminderHours,
      iconFileName: type === 'Hero' || type === 'CoreSettings' ? undefined : icon,
      soundEnabled: type === 'CoreSettings' ? undefined : soundEnabled,
      conditionalScriptBody: type === 'Conditional' ? (conditionalScript || null) : null,
      conditionalIntervalMinutes: type === 'Conditional' ? Number(conditionalInterval || '0') || null : null,
      dynamicScriptBody: type === 'Dynamic' ? (dynamicScript || null) : null,
      dynamicMaxLength: type === 'Dynamic' ? Number(dynamicMaxLength || '0') || null : null,
      dynamicTrimWhitespace: type === 'Dynamic' ? dynamicTrimWhitespace : null,
      dynamicFailIfEmpty: type === 'Dynamic' ? dynamicFailIfEmpty : null,
      dynamicFallbackMessage: type === 'Dynamic' ? (dynamicFallbackMessage || null) : null,
      heroFileName: type === 'Hero' && heroFile ? 'hero.png' : null,
      heroOriginalName: type === 'Hero' && heroFile ? heroFile.name : null,
      coreSettings: type === 'CoreSettings'
        ? {
          enabled: coreEnabled ? 1 : 0,
          autoClearModules: coreAutoClear ? 1 : 0,
          pollingIntervalSeconds: Number(corePolling || '0') || 300,
          heartbeatSeconds: Number(coreHeartbeat || '0') || 15,
          soundEnabled: coreSound ? 1 : 0,
          exitMenuVisible: coreExitVisible ? 1 : 0,
          startStopMenuVisible: coreStartStopVisible ? 1 : 0
        } as CoreSettingsBlock
        : null
    };

    try {
      setLoading(true);
      setGlobalStatus('Creating module...');
      const created = await createModule(apiBase, token, payload);
      setGlobalStatus('Created module.');

      let uploadSuccess = true;
      if (iconFile && created?.id && formType !== 'Hero' && formType !== 'CoreSettings') {
        try {
          setGlobalStatus('Uploading icon...');
          await uploadIcon(apiBase, token, created.id, iconFile);
          setIconPreviewUrl(getIconUrl(apiBase, created.id));
          setGlobalStatus('Created module and uploaded icon.');
        } catch (err: any) {
          uploadSuccess = false;
          setGlobalStatus(`Module created, but icon upload failed: ${err?.message ?? err}`);
        } finally {
          setIconFile(null);
        }
      }

      if (heroFile && created?.id && formType === 'Hero') {
        try {
          setGlobalStatus('Uploading hero image...');
          await uploadHero(apiBase, token, created.id, heroFile);
          setHeroPreviewUrl(getHeroUrl(apiBase, created.id));
          setGlobalStatus('Created module and uploaded hero image.');
        } catch (err: any) {
          uploadSuccess = false;
          setGlobalStatus(`Module created, but hero upload failed: ${err?.message ?? err}`);
        } finally {
          setHeroFile(null);
        }
      }

      if (uploadSuccess) {
        await loadModules();
        resetForm(formType);
      } else {
        // If upload failed, reload modules so they see the draft, but don't reset the form
        // so they can try fixing and re-saving (though they'd need to handle ModuleId conflict)
        await loadModules(true);
      }
    } catch (err: any) {
      setGlobalStatus(err?.message ?? 'Error creating module.');
    } finally {
      setLoading(false);
    }
  };

  return {
    modules,
    filteredModules,
    loading,
    search,
    setSearch,
    categoryFilter,
    setCategoryFilter,
    selectedIds,
    setSelectedIds,
    selectedModuleId,
    loadModules,
    formType,
    setFormType,
    formState,
    setFormState,
    resetForm,
    saveModule,
    setIconFile: (f: File | null) => {
      setIconFile(f);
      if (f) {
        const url = URL.createObjectURL(f);
        setIconPreviewUrl(url);
      } else {
        setIconPreviewUrl(null);
      }
    },
    setHeroFile: (f: File | null) => {
      setHeroFile(f);
      if (f) {
        const url = URL.createObjectURL(f);
        setLocalHeroPreview(url);
      } else {
        setLocalHeroPreview(null);
      }
    },
    iconPreviewUrl,
    heroPreviewUrl,
    heroLocalPreview: localHeroPreview,
    exportDevCoreSelected: async () => {
      if (!token) return setGlobalStatus('Login first.');
      if (!selectedModuleId) return setGlobalStatus('Select a module to export.');
      try {
        setLoading(true);
        setGlobalStatus('Exporting to Dev Core...');
        await exportDevCore(apiBase, token, selectedModuleId);
        setGlobalStatus('Exported to Dev Core.');
      } catch (err: any) {
        setGlobalStatus(err?.message ?? 'Export failed.');
      } finally {
        setLoading(false);
      }
    },
    exportModule: async (id: string) => {
      if (!token) return setGlobalStatus('Login first.');
      try {
        setLoading(true);
        setGlobalStatus('Exporting to Dev Core...');
        await exportDevCore(apiBase, token, id);
        setGlobalStatus('Exported to Dev Core.');
      } catch (err: any) {
        setGlobalStatus(err?.message ?? 'Export failed.');
      } finally {
        setLoading(false);
      }
    },
    removeSelected: async () => {
      if (!token) return setGlobalStatus('Login first.');
      if (selectedIds.size === 0) return setGlobalStatus('Select modules to remove.');
      try {
        setRemoving(true);
        setGlobalStatus('Removing selected modules...');
        for (const id of selectedIds) {
          await fetch(`${apiBase}/api/modules/${id}`, {
            method: 'DELETE',
            headers: { Authorization: `Bearer ${token}` }
          });
        }
        await loadModules();
        setSelectedIds(new Set());
        setGlobalStatus('Removed selected modules.');
      } catch (err: any) {
        setGlobalStatus(err?.message ?? 'Error removing modules.');
      } finally {
        setRemoving(false);
      }
    },
    removeModule: async (id: string) => {
      if (!token) return setGlobalStatus('Login first.');
      try {
        setRemoving(true);
        setGlobalStatus('Removing module...');
        await fetch(`${apiBase}/api/modules/${id}`, {
          method: 'DELETE',
          headers: { Authorization: `Bearer ${token}` }
        });
        await loadModules();
        setSelectedIds((prev) => {
          const next = new Set(prev);
          next.delete(id);
          return next;
        });
        setGlobalStatus('Removed module.');
      } catch (err: any) {
        setGlobalStatus(err?.message ?? 'Error removing module.');
      } finally {
        setRemoving(false);
      }
    }
  };
}
