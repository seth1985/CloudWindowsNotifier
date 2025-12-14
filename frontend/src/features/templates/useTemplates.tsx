import { useState } from 'react';
import type { PowerShellTemplate, TemplateType } from '../../types';
import { getTemplates, createTemplate, updateTemplate, deleteTemplate } from './api';

export function useTemplates(apiBase: string, token: string | null, setGlobalStatus: (s: string) => void) {
  const [mode, setMode] = useState<'conditional' | 'dynamic'>('conditional');
  const [templates, setTemplates] = useState<PowerShellTemplate[]>([]);
  const [loading, setLoading] = useState(false);
  const [showGallery, setShowGallery] = useState(false);
  const [showCreate, setShowCreate] = useState(false);
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [category, setCategory] = useState('General');
  const [type, setType] = useState<TemplateType>('Conditional');
  const [script, setScript] = useState('');
  const [editingId, setEditingId] = useState<string | null>(null);

  const loadTemplates = async (nextMode: 'conditional' | 'dynamic') => {
    if (!token) {
      setGlobalStatus('Login first to load templates.');
      return;
    }
    try {
      setLoading(true);
      const data = await getTemplates(apiBase, token, nextMode);
      setTemplates(data);
      setMode(nextMode);
      setGlobalStatus('Templates loaded.');
    } catch (err: any) {
      setGlobalStatus(err?.message ?? 'Error loading templates.');
    } finally {
      setLoading(false);
    }
  };

  const saveTemplate = async () => {
    if (!token) {
      setGlobalStatus('Login first.');
      return;
    }
    if (!title.trim() || !script.trim()) {
      setGlobalStatus('Title and script are required.');
      return;
    }
    const payload = {
      title: title.trim(),
      description: description.trim() || null,
      category: category.trim() || 'General',
      type,
      scriptBody: script
    };
    const isEdit = !!editingId;
    try {
      setLoading(true);
      let tpl: PowerShellTemplate;
      if (isEdit) {
        tpl = await updateTemplate(apiBase, token, editingId!, payload);
        setTemplates((prev) => prev.map((t) => (t.id === tpl.id ? tpl : t)));
        setGlobalStatus('Template updated.');
      } else {
        tpl = await createTemplate(apiBase, token, payload);
        setTemplates((prev) => [tpl, ...prev]);
        setGlobalStatus('Template saved.');
      }
      setShowCreate(false);
      setTitle('');
      setDescription('');
      setCategory('General');
      setScript('');
      setEditingId(null);
    } catch (err: any) {
      setGlobalStatus(err?.message ?? 'Error saving template.');
    } finally {
      setLoading(false);
    }
  };

  const beginEdit = (tpl: PowerShellTemplate) => {
    setEditingId(tpl.id);
    setTitle(tpl.title);
    setDescription(tpl.description ?? '');
    setCategory(tpl.category ?? 'General');
    setType(tpl.type);
    setScript(tpl.scriptBody ?? '');
    setShowCreate(true);
  };

  const removeTemplate = async (tpl: PowerShellTemplate) => {
    if (!token) {
      setGlobalStatus('Login first.');
      return;
    }
    try {
      setLoading(true);
      await deleteTemplate(apiBase, token, tpl.id);
      setTemplates((prev) => prev.filter((t) => t.id !== tpl.id));
      setGlobalStatus('Template removed.');
    } catch (err: any) {
      setGlobalStatus(err?.message ?? 'Error removing template.');
    } finally {
      setLoading(false);
    }
  };

  return {
    mode,
    setMode,
    templates,
    setTemplates,
    loading,
    showGallery,
    setShowGallery,
    showCreate,
    setShowCreate,
    title,
    setTitle,
    description,
    setDescription,
    category,
    setCategory,
    type,
    setType,
    script,
    setScript,
    loadTemplates,
    saveTemplate,
    beginEdit,
    removeTemplate,
    editingId,
    setEditingId
  };
}
