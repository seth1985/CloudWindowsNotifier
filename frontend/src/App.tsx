import React, { useEffect, useState } from 'react';
import { useAuth } from './features/auth/useAuth';
import { LoginPanel } from './features/auth/LoginPanel';
import { useModules } from './features/modules/useModules';
import { ModulesPage } from './features/modules/components/ModulesPage';
import { useTemplates } from './features/templates/useTemplates';
import { TemplateGalleryModal } from './features/templates/components/TemplateGalleryModal';
import { TemplateCreateModal } from './features/templates/components/TemplateCreateModal';
import { useTelemetry } from './features/telemetry/useTelemetry';
import { TelemetryOverview } from './features/telemetry/components/TelemetryOverview';
import type { NotificationType } from './types';

const DEFAULT_API_BASE = 'http://localhost:5210';

export default function App() {
  const [darkMode, setDarkMode] = useState<boolean>(() => localStorage.getItem('wnc_theme') === 'dark');

  const {
    apiBase,
    setApiBase,
    username,
    setUsername,
    password,
    setPassword,
    token,
    role,
    loading: authLoading,
    status,
    setStatus,
    handleLogin
  } = useAuth(DEFAULT_API_BASE);

  const authed = Boolean(token);
  const isAdvanced = role === 'Advanced';

  useEffect(() => {
    const stored = localStorage.getItem('wnc_api_base');
    if (stored) setApiBase(stored);
  }, []);

  useEffect(() => {
    localStorage.setItem('wnc_api_base', apiBase);
  }, [apiBase]);

  useEffect(() => {
    localStorage.setItem('wnc_theme', darkMode ? 'dark' : 'light');
  }, [darkMode]);

  const {
    modules,
    filteredModules,
    loading: modulesLoading,
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
    setIconFile,
    setHeroFile,
    iconPreviewUrl,
    heroLocalPreview,
    exportDevCoreSelected,
    exportModule,
    removeSelected,
    removeModule
  } = useModules(apiBase, token, setStatus, isAdvanced);

  const {
    mode: tplMode,
    templates,
    loading: templatesLoading,
    showGallery,
    setShowGallery,
    showCreate,
    setShowCreate,
    title: tplTitle,
    setTitle: setTplTitle,
    description: tplDescription,
    setDescription: setTplDescription,
    category: tplCategory,
    setCategory: setTplCategory,
    type: tplType,
    setType: setTplType,
    script: tplScript,
    setScript: setTplScript,
    editingId: tplEditingId,
    setEditingId: setTplEditingId,
    beginEdit: beginTplEdit,
    removeTemplate: removeTpl,
    loadTemplates,
    saveTemplate
  } = useTemplates(apiBase, token, setStatus);

  const {
    summary: telemetrySummary,
    perModule: telemetryModules,
    loading: telemetryLoading,
    loadTelemetry
  } = useTelemetry(apiBase, token, setStatus);

  const visibleTabs: NotificationType[] = isAdvanced
    ? ['Standard', 'Conditional', 'Dynamic', 'Hero', 'CoreSettings']
    : ['Standard', 'Conditional', 'Dynamic', 'Hero'];

  // Prevent lingering CoreSettings selection for non-advanced users
  useEffect(() => {
    if (!isAdvanced && formType === 'CoreSettings') {
      setFormType('Standard');
    }
  }, [isAdvanced, formType, setFormType]);

  const openTemplateGallery = () => {
    const mode: 'conditional' | 'dynamic' =
      formType === 'Dynamic' ? 'dynamic' : 'conditional';
    loadTemplates(mode);
    setShowGallery(true);
  };

  const handleCloseTemplateModal = () => {
    setShowCreate(false);
    setTplTitle('');
    setTplDescription('');
    setTplCategory('General');
    setTplType('Conditional');
    setTplScript('');
    setTplEditingId(null);
  };

  const handleInsertTemplate = (body: string) => {
    if (formType === 'Dynamic') {
      setFormState({ ...formState, dynamicScript: body });
    } else if (formType === 'Conditional') {
      setFormState({ ...formState, conditionalScript: body });
    }
    setStatus('Inserted template script.');
    setShowGallery(false);
  };

  const handleCopyTemplate = () => {
    setStatus('Copied script to clipboard (or attempted clipboard write).');
  };

  return (
    <div className={`page ${authed ? '' : 'unauth'} ${darkMode ? 'dark' : ''}`}>
      <nav className="top-nav">
        <div className="brand">Windows Notifier Cloud Admin</div>
        <div className="tabs type-tabs">
          {visibleTabs.map((t) => (
            <span
              key={t}
              className={`tab ${formType === t ? 'active' : ''}`}
              onClick={() => setFormType(t)}
            >
              {t === 'CoreSettings' ? 'Core Settings' : `${t} Notification`}
            </span>
          ))}
        </div>
        <div className="actions">
          <button onClick={() => setDarkMode((d) => !d)}>{darkMode ? 'Light mode' : 'Dark mode'}</button>
        </div>
      </nav>

      {!authed && (
        <LoginPanel
          apiBase={apiBase}
          setApiBase={setApiBase}
          username={username}
          setUsername={setUsername}
          password={password}
          setPassword={setPassword}
          loading={authLoading}
          status={status}
          onLogin={handleLogin}
        />
      )}

      {authed && (
        <>
          <header className="hero">
            <div>
              <h1>Cloud Admin Portal</h1>
              <p>Manage modules, monitor telemetry, and export to Dev Core.</p>
              <div className="connected">Connected to {apiBase} as {username}</div>
            </div>
            <div className="actions">
              <span className="actions-placeholder"></span>
            </div>
          </header>
          <div className={`status ${status ? 'visible' : ''}`}>{status}</div>

          <ModulesPage
            modules={modules}
            filteredModules={filteredModules}
            loading={modulesLoading}
            search={search}
            setSearch={setSearch}
            categoryFilter={categoryFilter}
            setCategoryFilter={setCategoryFilter}
            selectedIds={selectedIds}
            setSelectedIds={setSelectedIds}
            selectedModuleId={selectedModuleId}
            reload={loadModules}
            formType={formType}
            setFormType={setFormType}
            formState={formState as any}
            setFormState={setFormState as any}
            resetForm={resetForm}
            saveModule={saveModule}
            disableSave={!token || modulesLoading}
            onOpenTemplates={openTemplateGallery}
            onIconFileSelected={(f) => setIconFile(f)}
            onHeroFileSelected={(f) => setHeroFile(f)}
            iconPreviewUrl={iconPreviewUrl}
            heroPreviewUrl={heroLocalPreview}
            onExportSelected={exportDevCoreSelected}
            onExportRow={exportModule}
            onRemoveRow={removeModule}
            onRemoveSelected={removeSelected}
          />

          <TelemetryOverview
            summary={telemetrySummary}
            perModule={telemetryModules}
            loading={telemetryLoading}
            onRefresh={loadTelemetry}
          />

          <TemplateGalleryModal
            isOpen={showGallery}
            mode={tplMode}
            templates={templates}
            loading={templatesLoading}
            onClose={() => setShowGallery(false)}
            onRefresh={() => loadTemplates(tplMode)}
            onInsert={(tpl) => handleInsertTemplate(tpl.scriptBody)}
            onCopy={handleCopyTemplate}
            onCreateNew={() => setShowCreate(true)}
            onEdit={(tpl) => {
              beginTplEdit(tpl);
              setShowGallery(false);
            }}
            onRemove={(tpl) => {
              if (confirm(`Remove template \"${tpl.title}\"?`)) {
                removeTpl(tpl);
              }
            }}
          />

          <TemplateCreateModal
            isOpen={showCreate}
            onClose={handleCloseTemplateModal}
            title={tplTitle}
            setTitle={setTplTitle}
            description={tplDescription}
            setDescription={setTplDescription}
            category={tplCategory}
            setCategory={setTplCategory}
            type={tplType}
            setType={setTplType}
            script={tplScript}
            setScript={setTplScript}
            onSave={saveTemplate}
            loading={templatesLoading}
            isEditing={!!tplEditingId}
          />
        </>
      )}
    </div>
  );
}
