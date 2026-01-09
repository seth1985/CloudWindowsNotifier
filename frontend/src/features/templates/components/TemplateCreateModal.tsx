import React from 'react';
import { X, Save, Terminal } from 'lucide-react';
import type { TemplateType } from '../../types';
import { cn } from '../../../lib/utils';

type Props = {
  isOpen: boolean;
  onClose: () => void;
  isEditing?: boolean;
  title: string;
  setTitle: (s: string) => void;
  description: string;
  setDescription: (s: string) => void;
  category: string;
  setCategory: (s: string) => void;
  type: TemplateType;
  setType: (t: TemplateType) => void;
  script: string;
  setScript: (s: string) => void;
  onSave: () => void;
  loading: boolean;
};

export const TemplateCreateModal: React.FC<Props> = (props) => {
  if (!props.isOpen) return null;

  return (
    <div className="fixed inset-0 z-[60] flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm animate-in fade-in duration-300">
      <div className="bg-card border border-border w-full max-w-2xl rounded-2xl shadow-2xl overflow-hidden animate-in zoom-in-95 duration-300">
        {/* Header */}
        <div className="flex items-center justify-between p-6 border-b border-border bg-background/50">
          <div className="flex items-center gap-4">
            <div className="p-2.5 bg-primary-main/10 text-primary-main rounded-xl border border-primary-main/20">
              <Terminal className="w-5 h-5" />
            </div>
            <div className="flex flex-col">
              <h3 className="text-xl font-black text-text-primary tracking-tight uppercase">
                {props.isEditing ? 'Edit Template' : 'Create Template'}
              </h3>
              <p className="text-[10px] font-bold text-text-tertiary uppercase tracking-widest mt-0.5">
                Define reusable PowerShell logic
              </p>
            </div>
          </div>
          <button
            onClick={props.onClose}
            className="p-2 hover:bg-white/10 rounded-full transition-colors"
          >
            <X className="w-5 h-5 text-text-tertiary" />
          </button>
        </div>

        {/* Form Body */}
        <div className="p-6 space-y-6 max-h-[70vh] overflow-y-auto no-scrollbar">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div className="space-y-2.5">
              <label className="block text-[10px] font-black text-text-secondary uppercase tracking-[0.15em] ml-1">Template Title</label>
              <input
                className="input"
                placeholder="e.g. Check Disk Space"
                value={props.title}
                onChange={(e) => props.setTitle(e.target.value)}
              />
            </div>
            <div className="space-y-2.5">
              <label className="block text-[10px] font-black text-text-secondary uppercase tracking-[0.15em] ml-1">Category</label>
              <input
                className="input"
                placeholder="e.g. System"
                value={props.category}
                onChange={(e) => props.setCategory(e.target.value)}
              />
            </div>
          </div>

          <div className="space-y-2.5">
            <label className="block text-[10px] font-black text-text-secondary uppercase tracking-[0.15em] ml-1">Description</label>
            <input
              className="input"
              placeholder="Briefly describe what this script does..."
              value={props.description}
              onChange={(e) => props.setDescription(e.target.value)}
            />
          </div>

          <div className="space-y-2.5">
            <label className="block text-[10px] font-black text-text-secondary uppercase tracking-[0.15em] ml-1">Template Type</label>
            <select
              className="input appearance-none cursor-pointer"
              value={props.type}
              onChange={(e) => props.setType(e.target.value as TemplateType)}
            >
              <option value="Conditional">Conditional (Returns Boolean)</option>
              <option value="Dynamic">Dynamic (Returns String)</option>
              <option value="Both">Both</option>
            </select>
          </div>

          <div className="space-y-2.5">
            <div className="flex justify-between items-center ml-1">
              <label className="block text-[10px] font-black text-text-secondary uppercase tracking-[0.15em]">PowerShell Script</label>
              <span className="text-[9px] font-bold text-primary-main uppercase tracking-widest opacity-70">PS 5.1+ Compatible</span>
            </div>
            <textarea
              className="input h-64 font-mono text-xs resize-none leading-relaxed bg-black/10 border-border/50 focus:bg-black/20 transition-all no-scrollbar"
              placeholder="# Enter your PowerShell code here...&#10;return $true"
              value={props.script}
              onChange={(e) => props.setScript(e.target.value)}
            />
          </div>
        </div>

        {/* Footer Actions */}
        <div className="p-6 border-t border-border flex justify-end gap-3 bg-background/30">
          <button
            onClick={props.onClose}
            className="btn btn-secondary px-6 h-11 text-xs"
            disabled={props.loading}
          >
            Cancel
          </button>
          <button
            onClick={props.onSave}
            disabled={props.loading}
            className="btn btn-primary px-8 h-11 text-xs gap-2"
          >
            {props.loading ? (
              <RefreshCcw className="w-4 h-4 animate-spin" />
            ) : (
              <Save className="w-4 h-4" />
            )}
            {props.isEditing ? 'Update Template' : 'Save Template'}
          </button>
        </div>
      </div>
    </div>
  );
};
