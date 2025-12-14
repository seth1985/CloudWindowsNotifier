import React, { useState } from 'react';
import type { PowerShellTemplate } from '../../types';

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
    <div className="modal-backdrop">
      <div className="modal template-modal">
        <div className="modal-head">
          <h3>PowerShell Templates ({props.mode === 'conditional' ? 'Conditional' : 'Dynamic'})</h3>
          <div className="modal-actions">
            <button onClick={props.onRefresh} disabled={props.loading}>Refresh</button>
            <button onClick={props.onCreateNew}>Create New</button>
            <button onClick={props.onClose}>Close</button>
          </div>
        </div>
        {props.loading && <div className="hint">Loading templates...</div>}
        <div className="template-list">
          {Object.keys(grouped).length === 0 && !props.loading && <div className="hint">No templates found.</div>}
          {Object.entries(grouped).map(([category, items]) => (
            <div key={category} className="template-group">
              <h4>{category}</h4>
              <div className="template-cards">
                {items.map((tpl) => {
                  const expanded = expandedId === tpl.id;
                  return (
                    <div key={tpl.id} className="template-card">
                      <div className="template-card-head">
                        <div>
                          <div className="template-title">{tpl.title}</div>
                          <div className="template-desc">{tpl.description}</div>
                        </div>
                        <div className="template-tags">
                          <span className="chip">{tpl.type}</span>
                        </div>
                      </div>
                      <div className="template-actions">
                        <button onClick={() => props.onInsert(tpl)}>Insert</button>
                        <button onClick={async () => {
                          try {
                            await navigator.clipboard.writeText(tpl.scriptBody);
                            props.onCopy(tpl);
                          } catch {
                            props.onCopy(tpl);
                          }
                        }}>Copy</button>
                        <button onClick={() => setExpandedId(expanded ? null : tpl.id)}>
                          {expanded ? 'Hide script' : 'Show script'}
                        </button>
                      </div>
                      {expanded && (
                        <pre className="template-script">{tpl.scriptBody}</pre>
                      )}
                    </div>
                  );
                })}
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
};
