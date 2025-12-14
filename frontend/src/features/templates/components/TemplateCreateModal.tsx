import React from 'react';
import type { TemplateType } from '../../types';

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
    <div className="modal-backdrop">
      <div className="modal template-modal">
        <div className="modal-head">
          <h3>{props.isEditing ? 'Edit template' : 'Create template'}</h3>
          <div className="modal-actions">
            <button onClick={props.onClose}>Close</button>
          </div>
        </div>
        <label>
          Title
          <input value={props.title} onChange={(e) => props.setTitle(e.target.value)} />
        </label>
        <label>
          Description
          <input value={props.description} onChange={(e) => props.setDescription(e.target.value)} />
        </label>
        <label>
          Category
          <input value={props.category} onChange={(e) => props.setCategory(e.target.value)} />
        </label>
        <label>
          Type
          <select value={props.type} onChange={(e) => props.setType(e.target.value as TemplateType)}>
            <option value="Conditional">Conditional</option>
            <option value="Dynamic">Dynamic</option>
            <option value="Both">Both</option>
          </select>
        </label>
        <label>
          Script
          <textarea
            className="script-area"
            placeholder="Enter PowerShell script..."
            value={props.script}
            onChange={(e) => props.setScript(e.target.value)}
          />
        </label>
        <div className="modal-actions">
          <button className="btn primary" onClick={props.onSave} disabled={props.loading}>
            {props.isEditing ? 'Update template' : 'Save template'}
          </button>
        </div>
      </div>
    </div>
  );
};
