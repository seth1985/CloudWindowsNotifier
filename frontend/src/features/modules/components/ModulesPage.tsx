import React, { useState } from 'react';
import { Search, Filter, MoreHorizontal, Download, Trash2, CheckSquare, Square, RefreshCcw, ArrowUpDown } from 'lucide-react';
import type { ModuleRow, NotificationType } from '../../../types';
import { ModuleForm } from './ModuleForm';
import { cn } from '../../../lib/utils';

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
  isAdvanced?: boolean;
  isAdmin?: boolean;
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
    onRemoveRow,
    isAdvanced = false,
    isAdmin = false
  } = props;

  // Simple Pagination State
  const [currentPage, setCurrentPage] = useState(1);
  const itemsPerPage = 8;
  const totalPages = Math.ceil(filteredModules.length / itemsPerPage);
  const paginatedModules = filteredModules.slice(
    (currentPage - 1) * itemsPerPage,
    currentPage * itemsPerPage
  );

  const toggleSelection = (id: string) => {
    const next = new Set(selectedIds);
    if (next.has(id)) next.delete(id);
    else next.add(id);
    setSelectedIds(next);
  };

  const categories = ['All', 'GeneralInfo', 'Security', 'Compliance', 'Maintenance', 'Application'];

  return (
    <div className="space-y-8 animate-in fade-in duration-500">
      {/* Animated Background Blobs (subtle) */}
      <div className="fixed inset-0 -z-10 overflow-hidden pointer-events-none">
        <div className="absolute top-1/4 -left-20 w-96 h-96 bg-primary-main/5 blur-[120px] animate-blob"></div>
        <div className="absolute bottom-1/4 -right-20 w-96 h-96 bg-purple-500/5 blur-[120px] animate-blob animation-delay-2000"></div>
      </div>

      {/* Module Editor Form */}
      <div className="card shadow-lg border-border/50">
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
          isAdvanced={isAdvanced}
          isAdmin={isAdmin}
        />
      </div>

      {/* Main Table Card */}
      <div className="card shadow-xl flex flex-col p-0 border-border overflow-hidden">

        {/* Toolbar */}
        <div className="p-6 border-b border-border flex flex-col md:flex-row gap-6 justify-between items-start md:items-center bg-background/40">

          <div className="flex flex-col gap-5 w-full md:w-auto">
            <div className="relative group">
              <Search className="absolute left-3.5 top-1/2 -translate-y-1/2 w-4 h-4 text-text-tertiary group-focus-within:text-primary-main transition-colors" />
              <input
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                placeholder="Search modules..."
                className="input pl-11 h-11 w-full md:w-96"
              />
            </div>

            <div className="flex gap-2 flex-wrap">
              {categories.map(c => (
                <button
                  key={c}
                  onClick={() => setCategoryFilter(c)}
                  className={cn(
                    "px-4 py-2 rounded-full text-xs font-black uppercase tracking-widest transition-all border",
                    categoryFilter === c
                      ? "bg-primary-main text-white border-primary-main shadow-lg"
                      : "bg-surface-chip text-text-secondary border-border hover:bg-surface-hover hover:text-text-primary"
                  )}
                >
                  {c === 'All' ? 'All' : c.replace('Info', '')}
                </button>
              ))}
            </div>
          </div>

          <div className="flex items-center gap-3 w-full md:w-auto justify-end">
            <button
              onClick={reload}
              disabled={loading}
              className="btn btn-secondary h-11 w-11 p-0 min-w-0"
              title="Refresh"
            >
              <RefreshCcw className={cn("w-4 h-4", loading && "animate-spin")} />
            </button>
            <div className="h-6 w-px bg-border mx-2" />

            {selectedIds.size > 0 && (
              <div className="flex items-center gap-3 animate-in slide-in-from-right-2 duration-300">
                <button
                  onClick={onExportSelected}
                  className="btn btn-secondary h-11 px-6 text-xs"
                >
                  <Download className="w-4 h-4" />
                  Export ({selectedIds.size})
                </button>
                <button
                  onClick={onRemoveSelected}
                  className="btn h-11 px-6 text-xs bg-red-500/10 text-red-600 border-red-500/30 hover:bg-red-500/20 hover:border-red-500/50"
                >
                  <Trash2 className="w-4 h-4" />
                  Delete ({selectedIds.size})
                </button>
              </div>
            )}
            <button
              className="h-11 px-6 text-xs font-black uppercase tracking-widest text-text-tertiary border border-dashed border-border rounded-xl cursor-not-allowed hidden md:flex items-center gap-2 opacity-50"
              disabled
            >
              Publish (Soon)
            </button>
          </div>
        </div>

        {/* Table */}
        <div className="overflow-x-auto min-h-[400px]">
          <table className="w-full text-left text-sm border-collapse">
            <thead>
              <tr className="bg-background/50 border-b border-border">
                <th className="px-6 py-5 w-16 text-center">
                  <button
                    onClick={() => {
                      if (selectedIds.size === filteredModules.length && filteredModules.length > 0) {
                        setSelectedIds(new Set());
                      } else {
                        setSelectedIds(new Set(filteredModules.map(m => m.id)));
                      }
                    }}
                    className="text-text-tertiary hover:text-primary-main transition-colors p-1"
                  >
                    {filteredModules.length > 0 && selectedIds.size === filteredModules.length
                      ? <CheckSquare className="w-5 h-5" />
                      : <Square className="w-5 h-5" />}
                  </button>
                </th>
                <th className="px-6 py-5 font-black text-text-secondary uppercase tracking-[0.15em] text-[10px]">
                  <div className="flex items-center gap-2 cursor-pointer hover:text-text-primary transition-colors">
                    Name <ArrowUpDown className="w-3 h-3" />
                  </div>
                </th>
                <th className="px-6 py-5 font-black text-text-secondary uppercase tracking-[0.15em] text-[10px]">Type</th>
                <th className="px-6 py-5 font-black text-text-secondary uppercase tracking-[0.15em] text-[10px]">Category</th>
                <th className="px-6 py-5 font-black text-text-secondary uppercase tracking-[0.15em] text-[10px]">Version</th>
                <th className="px-6 py-5 font-black text-text-secondary uppercase tracking-[0.15em] text-[10px]">Status</th>
                <th className="px-6 py-5 font-black text-text-secondary uppercase tracking-[0.15em] text-[10px] text-right">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-border/50">
              {filteredModules.length === 0 ? (
                <tr>
                  <td colSpan={8} className="px-6 py-20 text-center text-text-tertiary">
                    <div className="flex flex-col items-center gap-4">
                      <div className="p-4 bg-surface-chip rounded-full">
                        <Filter className="w-8 h-8 opacity-40 text-primary-main" />
                      </div>
                      <div className="space-y-1">
                        <p className="font-bold text-text-primary text-lg">No modules found</p>
                        <p className="text-sm">Try adjusting your filters or search terms.</p>
                      </div>
                      <button onClick={() => { setSearch(''); setCategoryFilter('All'); }} className="btn btn-secondary mt-2 px-6">
                        Clear all filters
                      </button>
                    </div>
                  </td>
                </tr>
              ) : (
                paginatedModules.map((m) => (
                  <tr key={m.id} className={cn(
                    "transition-all group",
                    selectedIds.has(m.id) ? "bg-primary-main/5 hover:bg-primary-main/10" : "hover:bg-surface-hover"
                  )}
                  >
                    <td className="px-6 py-4 text-center">
                      <button onClick={() => toggleSelection(m.id)} className={cn("transition-all duration-200 hover:scale-110", selectedIds.has(m.id) ? "text-primary-main" : "text-text-tertiary group-hover:text-text-secondary")}>
                        {selectedIds.has(m.id) ? <CheckSquare className="w-5 h-5" /> : <Square className="w-5 h-5" />}
                      </button>
                    </td>
                    <td className="px-6 py-4 font-bold text-text-primary group-hover:text-primary-main transition-colors">{m.displayName}</td>
                    <td className="px-6 py-4 font-bold text-text-secondary text-xs">{m.type}</td>
                    <td className="px-6 py-4">
                      <span className="inline-flex items-center px-3 py-1.5 rounded-lg text-[10px] font-black uppercase tracking-widest bg-surface-chip text-text-secondary border border-border group-hover:border-primary-main/30 group-hover:text-primary-main transition-all">
                        {m.category}
                      </span>
                    </td>
                    <td className="px-6 py-4 font-mono text-text-tertiary font-bold text-xs">{m.version}</td>
                    <td className="px-6 py-4">
                      <span className={cn(
                        "inline-flex items-center px-3 py-1.5 rounded-lg text-[10px] font-black uppercase tracking-widest border transition-all",
                        m.isPublished
                          ? "bg-green-500/10 text-green-700 border-green-500/30 dark:text-green-400 dark:border-green-500/20"
                          : "bg-surface-chip text-text-tertiary border-border"
                      )}>
                        {m.isPublished ? 'Published' : 'Draft'}
                      </span>
                    </td>
                    <td className="px-6 py-4 text-right">
                      <div className="flex items-center justify-end gap-2 opacity-0 group-hover:opacity-100 transition-all translate-x-3 group-hover:translate-x-0">
                        <button onClick={() => onExportRow(m.id)} className="btn btn-secondary p-0 h-12 w-12 min-w-0 hover:text-blue-500 hover:border-blue-500/50 hover:bg-blue-500/5" title="Export">
                          <Download className="w-6 h-6" />
                        </button>
                        <button onClick={() => onRemoveRow(m.id)} className="btn p-0 h-12 w-12 min-w-0 bg-red-500/5 text-red-500/70 border-red-500/20 hover:text-red-500 hover:bg-red-500/10 hover:border-red-500/40 transition-all" title="Delete">
                          <Trash2 className="w-6 h-6" />
                        </button>
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>

        {/* Footer / Pagination */}
        <div className="p-6 border-t border-border flex items-center justify-between text-xs font-bold text-text-tertiary bg-background/40">
          <div>
            Showing <span className="text-text-primary">{(currentPage - 1) * itemsPerPage + 1}</span> to <span className="text-text-primary">{Math.min(currentPage * itemsPerPage, filteredModules.length)}</span> of <span className="text-text-primary">{filteredModules.length}</span> results
          </div>
          <div className="flex gap-4">
            <button
              disabled={currentPage === 1}
              onClick={() => setCurrentPage(p => Math.max(1, p - 1))}
              className="btn btn-secondary px-5 py-2 text-xs disabled:opacity-40 disabled:grayscale transition-all"
            >
              Previous
            </button>
            <div className="flex gap-2">
              {Array.from({ length: totalPages }, (_, i) => i + 1).map(p => (
                <button
                  key={p}
                  onClick={() => setCurrentPage(p)}
                  className={cn(
                    "w-10 h-10 rounded-xl flex items-center justify-center font-black transition-all border",
                    currentPage === p
                      ? "bg-primary-main text-white border-primary-main shadow-md"
                      : "bg-surface-chip border-border text-text-tertiary hover:bg-surface-hover hover:text-text-primary"
                  )}
                >
                  {p}
                </button>
              ))}
            </div>
            <button
              disabled={currentPage === totalPages || totalPages === 0}
              onClick={() => setCurrentPage(p => Math.min(totalPages, p + 1))}
              className="btn btn-secondary px-5 py-2 text-xs disabled:opacity-40 disabled:grayscale transition-all"
            >
              Next
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};
