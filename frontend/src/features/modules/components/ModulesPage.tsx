import React from 'react';
import type { ModuleRow } from '../../types';
import { ModuleForm } from './ModuleForm';
import type { NotificationType } from '../../types';

type FormState = React.ComponentProps<typeof ModuleForm>['formState'];

type Props = {
  modules: ModuleRow[];
  filteredModules: ModuleRow[];
  loading: boolean;
  search: string;
  setSearch: (v: string) => void;
  categoryFilter: string;
  setCategoryFilter: (v: string) => void;
  selectedIds: Set<string>;
  setSelectedIds: (s: Set<string>) => void;
  selectedModuleId: string | null;
  reload: () => void;
  formType: NotificationType;
  setFormType: (t: NotificationType) => void;
  formState: FormState;
  setFormState: (s: FormState) => void;
  resetForm: (keepType?: NotificationType) => void;
  saveModule: () => void;
  disableSave: boolean;
  onOpenTemplates: () => void;
  onIconFileSelected: (f: File) => void;
  onHeroFileSelected: (f: File) => void;
  iconPreviewUrl?: string | null;
  heroPreviewUrl?: string | null;
  onExportSelected: () => void;
  onRemoveSelected: () => void;
  onExportRow: (id: string) => void;
  onRemoveRow: (id: string) => void;
};

export const ModulesPage: React.FC<Props> = (props) => {
  const {
    filteredModules,
    loading,
    search,
    setSearch,
    categoryFilter,
    setCategoryFilter,
    selectedIds,
    setSelectedIds,
    selectedModuleId,
    reload,
    formType,
    setFormType,
    formState,
    setFormState,
    resetForm,
    saveModule,
    disableSave,
    onOpenTemplates,
    onIconFileSelected,
    onHeroFileSelected,
    iconPreviewUrl,
    heroPreviewUrl,
    onExportSelected,
    onRemoveSelected,
    onExportRow,
    onRemoveRow
  } = props;

  return (
    <section className="card stack">
      <ModuleForm
        formType={formType}
        setFormType={setFormType}
        formState={formState}
        setFormState={setFormState}
        onSave={saveModule}
        onNew={() => resetForm(formType)}
        disableSave={disableSave}
        onOpenTemplates={onOpenTemplates}
        onIconFileSelected={onIconFileSelected}
        onHeroFileSelected={onHeroFileSelected}
        iconPreviewUrl={iconPreviewUrl}
        heroPreviewUrl={heroPreviewUrl}
      />

      <div className="filters-row">
        <div className="filters">
          <input
            className="search"
            placeholder="Search by name or id"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
          <div className="chips">
            {['All', 'GeneralInfo', 'Security', 'Compliance', 'Maintenance', 'Application'].map((c) => (
              <span
                key={c}
                className={`chip ${categoryFilter === c ? 'active' : ''}`}
                onClick={() => setCategoryFilter(c)}
              >
                {c === 'All' ? 'All Categories' : c.replace('Info', '')}
              </span>
            ))}
          </div>
        </div>
        <div className="filters-actions">
          <span className="actions-placeholder"></span>
          <button disabled={!selectedModuleId || loading} onClick={onExportSelected}>
            Export to Dev Core
          </button>
          <button disabled className="muted-btn">
            Publish (coming soon)
          </button>
        </div>
      </div>

      <div className="table-wrapper">
        <table>
          <thead>
            <tr>
              <th></th>
              <th>Name</th>
              <th>ModuleId</th>
              <th>Type</th>
              <th>Category</th>
              <th>Version</th>
              <th>Status</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {filteredModules.map((m) => (
              <tr key={m.id}>
                <td>
                  <input
                    type="checkbox"
                    checked={selectedIds.has(m.id)}
                    onChange={(e) => {
                      const next = new Set(selectedIds);
                      if (e.target.checked) next.add(m.id);
                      else next.delete(m.id);
                      setSelectedIds(next);
                    }}
                  />
                </td>
                <td>{m.displayName}</td>
              <td>{m.moduleId}</td>
              <td>{m.type}</td>
              <td>{m.category}</td>
              <td>{m.version}</td>
                <td>
                  <span className={`badge ${m.isPublished ? 'success' : 'muted'}`}>
                    {m.isPublished ? 'Published' : 'Draft'}
                  </span>
                </td>
                <td className="actions-cell">
                  <button onClick={() => onExportRow(m.id)} disabled={loading}>
                    Export
                  </button>
                  <button onClick={() => onRemoveRow(m.id)} disabled={loading}>
                    Remove
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
        {filteredModules.length === 0 && <div className="empty">No modules match your filters.</div>}
      </div>

      <div className="table-actions">
        <button onClick={reload} disabled={loading}>
          Refresh modules
        </button>
        <div className="bulk-bar align-right">
          <div className="bulk-actions">
            <button
              onClick={() => setSelectedIds(new Set(filteredModules.map((m) => m.id)))}
              disabled={filteredModules.length === 0}
            >
              Check All
            </button>
            <button onClick={() => setSelectedIds(new Set())} disabled={selectedIds.size === 0}>
              Uncheck All
            </button>
            <button onClick={onRemoveSelected} disabled={selectedIds.size === 0 || loading}>
              Remove checked
            </button>
          </div>
          <div className="bulk-status">Selected: {selectedIds.size}</div>
        </div>
      </div>
    </section>
  );
};
