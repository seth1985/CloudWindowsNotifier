import React, { useState } from 'react';
import { X, RefreshCcw, Plus, Code, Copy, Trash2, ChevronDown, ChevronUp } from 'lucide-react';
import type { PowerShellTemplate } from '../../types';
import { cn } from '../../../lib/utils';

type Props = {
  isOpen: boolean;
  mode: 'conditional' | 'dynamic';
  templates: PowerShellTemplate[];
  loading: boolean;
  onClose: () => void;
  onRefresh: () => void;
  onInsert: (tpl: PowerShellTemplate) => void;
  onCopy: (tpl: PowerShellTemplate) => void;
  onCreateNew: () => void;
  onEdit: (tpl: PowerShellTemplate) => void;
  onRemove: (tpl: PowerShellTemplate) => void;
};

export const TemplateGalleryModal: React.FC<Props> = (props) => {
  const [expandedId, setExpandedId] = useState<string | null>(null);

  if (!props.isOpen) return null;

  const grouped = props.templates.reduce<Record<string, PowerShellTemplate[]>>((acc, tpl) => {
    const key = tpl.category || 'Uncategorized';
    if (!acc[key]) acc[key] = [];
    acc[key].push(tpl);
    return acc;
  }, {});

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm animate-in fade-in duration-300">
      <div className="bg-card border border-border w-full max-w-4xl max-h-[90vh] rounded-2xl shadow-2xl flex flex-col overflow-hidden animate-in zoom-in-95 duration-300">
        {/* Header */}
        <div className="flex items-center justify-between p-6 border-b border-border bg-background/50">
          <div className="flex flex-col">
            <h3 className="text-xl font-black text-text-primary tracking-tight uppercase">
              PowerShell Templates
            </h3>
            <p className="text-[10px] font-bold text-primary-main uppercase tracking-widest mt-0.5">
              {props.mode} logic layer library
            </p>
          </div>
          <div className="flex items-center gap-3">
            <button
              onClick={props.onRefresh}
              disabled={props.loading}
              className="btn btn-secondary p-2 h-10 w-10"
              title="Refresh Library"
            >
              <RefreshCcw className={cn("w-4 h-4", props.loading && "animate-spin")} />
            </button>
            <button
              onClick={props.onCreateNew}
              className="btn btn-primary px-4 h-10 text-xs gap-2"
            >
              <Plus className="w-4 h-4" />
              New Template
            </button>
            <div className="w-px h-6 bg-border mx-1" />
            <button
              onClick={props.onClose}
              className="p-2 hover:bg-white/10 rounded-full transition-colors"
            >
              <X className="w-5 h-5 text-text-tertiary" />
            </button>
          </div>
        </div>

        {/* Content */}
        <div className="flex-1 overflow-y-auto p-6 space-y-8 no-scrollbar">
          {props.loading && (
            <div className="flex flex-col items-center justify-center py-12 gap-3 text-text-tertiary">
              <RefreshCcw className="w-8 h-8 animate-spin opacity-50" />
              <p className="text-sm font-bold uppercase tracking-widest">Loading Library...</p>
            </div>
          )}

          {!props.loading && Object.keys(grouped).length === 0 && (
            <div className="flex flex-col items-center justify-center py-20 bg-surface-chip/10 rounded-2xl border border-dashed border-border gap-4">
              <Code className="w-12 h-12 text-text-tertiary opacity-30" />
              <div className="text-center">
                <p className="text-lg font-bold text-text-primary">No templates found</p>
                <p className="text-sm text-text-tertiary">Start by creating a new script template.</p>
              </div>
              <button onClick={props.onCreateNew} className="btn btn-secondary mt-2 px-6">
                Create First Template
              </button>
            </div>
          )}

          {!props.loading && Object.entries(grouped).map(([category, items]) => (
            <div key={category} className="space-y-4">
              <h4 className="text-[10px] font-black text-text-secondary uppercase tracking-[0.2em] px-1 sticky top-0 bg-card py-1 z-10">
                {category}
              </h4>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                {items.map((tpl) => {
                  const expanded = expandedId === tpl.id;
                  return (
                    <div key={tpl.id} className="card p-5 group hover:border-primary-main/50 transition-all flex flex-col gap-4 border-border/70">
                      <div className="flex justify-between items-start">
                        <div className="space-y-1">
                          <div className="text-sm font-black text-text-primary group-hover:text-primary-main transition-colors uppercase tracking-tight">{tpl.title}</div>
                          <div className="text-xs text-text-tertiary font-medium line-clamp-2">{tpl.description}</div>
                        </div>
                        <span className="text-[10px] font-black uppercase tracking-widest px-2 py-1 rounded-md bg-surface-chip text-text-secondary border border-border">
                          {tpl.type}
                        </span>
                      </div>

                      <div className="flex items-center gap-3 pt-2 border-t border-border/50">
                        <button
                          onClick={() => props.onInsert(tpl)}
                          className="flex-1 btn btn-primary py-2.5 text-[10px] uppercase tracking-widest h-11"
                        >
                          Insert
                        </button>

                        <div className="flex items-center gap-2">
                          <button
                            onClick={async () => {
                              try {
                                await navigator.clipboard.writeText(tpl.scriptBody);
                                props.onCopy(tpl);
                              } catch {
                                props.onCopy(tpl);
                              }
                            }}
                            className="btn btn-secondary p-0 h-11 w-11 hover:text-blue-500 hover:border-blue-500/50 hover:bg-blue-500/5"
                            title="Copy Script"
                          >
                            <Copy className="w-5 h-5" />
                          </button>

                          <button
                            onClick={() => props.onEdit(tpl)}
                            className="btn btn-secondary p-0 h-11 w-11 hover:text-purple-500 hover:border-purple-500/50 hover:bg-purple-500/5"
                            title="Edit"
                          >
                            <Code className="w-5 h-5" />
                          </button>

                          <button
                            onClick={() => props.onRemove(tpl)}
                            className="btn p-0 h-11 w-11 bg-red-500/5 text-red-500/70 border-red-500/20 hover:text-red-500 hover:bg-red-500/10 hover:border-red-500/40 transition-all"
                            title="Remove"
                          >
                            <Trash2 className="w-5 h-5" />
                          </button>
                        </div>
                      </div>

                      <button
                        onClick={() => setExpandedId(expanded ? null : tpl.id)}
                        className="flex items-center justify-center gap-2 text-[10px] font-black uppercase tracking-widest text-text-tertiary hover:text-text-primary transition-colors py-1"
                      >
                        {expanded ? <ChevronUp className="w-3 h-3" /> : <ChevronDown className="w-3 h-3" />}
                        {expanded ? 'Hide Source' : 'View Source'}
                      </button>

                      {expanded && (
                        <div className="animate-in slide-in-from-top-2 duration-200">
                          <pre className="text-[10px] font-mono p-4 bg-black/20 rounded-xl border border-border/50 text-primary-main/90 overflow-x-auto whitespace-pre-wrap max-h-40 no-scrollbar">
                            {tpl.scriptBody}
                          </pre>
                        </div>
                      )}
                    </div>
                  );
                })}
              </div>
            </div>
          ))}
        </div>

        {/* Footer */}
        <div className="p-4 bg-surface-chip/20 border-t border-border flex justify-center">
          <p className="text-[10px] font-bold text-text-tertiary uppercase tracking-[0.15em]">
            Select a template to inject it into your module logic layer
          </p>
        </div>
      </div>
    </div>
  );
};
