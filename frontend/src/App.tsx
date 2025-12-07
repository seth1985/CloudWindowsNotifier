import { useEffect, useMemo, useState } from 'react';

type ModuleRow = {
  id: string;
  displayName: string;
  moduleId: string;
  type: string;
  category: string;
  campaign?: string;
  description?: string;
  version: number;
  isPublished: boolean;
  iconFileName?: string | null;
  iconOriginalName?: string | null;
   heroFileName?: string | null;
   heroOriginalName?: string | null;
};

export default function App() {
  const [apiBase, setApiBase] = useState('http://localhost:5210');
  const [username, setUsername] = useState('admin');
  const [password, setPassword] = useState('P@ssw0rd!');
  const [token, setToken] = useState<string | null>(null);
  const [modules, setModules] = useState<ModuleRow[]>([]);
  const [loading, setLoading] = useState(false);
  const [status, setStatus] = useState<string>('');
  const [telemetry, setTelemetry] = useState<any>(null);
  const [telemetryModules, setTelemetryModules] = useState<any[]>([]);

  const [search, setSearch] = useState('');
  const [categoryFilter, setCategoryFilter] = useState('All');
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());

  const [newName, setNewName] = useState('');
  const [newModuleId, setNewModuleId] = useState(generateModuleId(''));
  const [newTitle, setNewTitle] = useState('');
  const [newMessage, setNewMessage] = useState('');
  const [newCategory, setNewCategory] = useState('GeneralInfo');
  const [newType, setNewType] = useState<'Standard' | 'Conditional' | 'Dynamic' | 'Hero'>('Standard');
  const [newLinkUrl, setNewLinkUrl] = useState('https://');
  const [newSchedule, setNewSchedule] = useState('');
  const [newExpires, setNewExpires] = useState('');
  const [newReminderHours, setNewReminderHours] = useState('1');
  const [newIcon, setNewIcon] = useState('info.ico');
  const [newCustomIcon, setNewCustomIcon] = useState('info.ico');
  const [pendingHeroFile, setPendingHeroFile] = useState<File | null>(null);
  const [pendingHeroPreview, setPendingHeroPreview] = useState<string | null>(null);
  const [heroMeta, setHeroMeta] = useState<string>('');
  const [newConditionalScript, setNewConditionalScript] = useState('');
  const [newDynamicScript, setNewDynamicScript] = useState('');
  const [dynamicMaxLength, setDynamicMaxLength] = useState('240');
  const [dynamicTrimWhitespace, setDynamicTrimWhitespace] = useState(true);
  const [dynamicFailIfEmpty, setDynamicFailIfEmpty] = useState(true);
  const [dynamicFallbackMessage, setDynamicFallbackMessage] = useState('');
  const [conditionalInterval, setConditionalInterval] = useState('60');
  const [soundEnabled, setSoundEnabled] = useState(true);
  const [showIconPicker, setShowIconPicker] = useState(false);
  const [iconPreview, setIconPreview] = useState<string | null>('/icons/emblems/info.ico');
  const [pendingIconFile, setPendingIconFile] = useState<File | null>(null);
  const [pendingIconPreview, setPendingIconPreview] = useState<string | null>(null);
  const [darkMode, setDarkMode] = useState<boolean>(() => localStorage.getItem('wnc_theme') === 'dark');

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

  useEffect(() => {
    if (newType === 'Conditional')
    {
      if (!conditionalInterval) setConditionalInterval('60');
    }
    if (newType === 'Dynamic')
    {
      if (!dynamicMaxLength) setDynamicMaxLength('240');
    }
  }, [newType]);

const filteredModules = useMemo(() => {
  const term = search.trim().toLowerCase();
  return modules.filter((m) => {
    const matchesTerm =
      !term ||
      m.displayName.toLowerCase().includes(term) ||
      m.moduleId.toLowerCase().includes(term) ||
      (m.campaign ?? '').toLowerCase().includes(term);
    const matchesCat = categoryFilter === 'All' || m.category === categoryFilter;
    return matchesTerm && matchesCat;
  });
}, [modules, search, categoryFilter]);

  const authed = Boolean(token);
  const selectedModuleId = selectedIds.size === 1 ? Array.from(selectedIds)[0] : null;
  const handleSaveDraft = () => {
    const nameTrim = newName.trim();
    const moduleIdTrim = newModuleId.trim();
    const titleTrim = newTitle.trim();
    const messageTrim = newMessage.trim();
    const dynScriptTrim = newDynamicScript.trim();

    if (!nameTrim) {
      setStatus('Name is required.');
      return;
    }
    if (!moduleIdTrim) {
      setStatus('Module ID is required.');
      return;
    }
    if (!titleTrim) {
      setStatus('Title is required.');
      return;
    }
    if (newType === 'Standard' || newType === 'Conditional') {
      if (!messageTrim) {
        setStatus('Message is required for this module type.');
        return;
      }
    }
    if (newType === 'Dynamic' && !dynScriptTrim) {
      setStatus('Dynamic script is required.');
      return;
    }
    if (newType === 'Hero' && !pendingHeroFile) {
      setStatus('Hero image is required for Hero notifications.');
      return;
    }
    createModule(
      apiBase,
      token,
      newName,
      newModuleId,
      newTitle,
      newMessage,
      newCategory,
      newLinkUrl,
      newSchedule,
      newExpires,
      newReminderHours,
      setStatus,
      setModules,
      setLoading,
      setNewModuleId,
      newIcon,
      pendingIconFile,
      pendingIconPreview,
      setPendingIconFile,
      setPendingIconPreview,
      setNewCustomIcon,
      setIconPreview,
      soundEnabled,
      newType,
      newConditionalScript,
      newDynamicScript,
      dynamicMaxLength,
      dynamicTrimWhitespace,
      dynamicFailIfEmpty,
      dynamicFallbackMessage,
      conditionalInterval,
      pendingHeroFile,
      pendingHeroPreview,
      setPendingHeroFile,
      setPendingHeroPreview,
      setHeroMeta
    ).then(() => resetForm(newType));
  };

  const handleExportSelected = () => {
    if (!selectedModuleId) {
      setStatus('Select a module to export.');
      return;
    }
    exportDevCore(apiBase, token, selectedModuleId, setStatus, setLoading);
  };
  const handleIconFileSelected = (file: File) => {
    // If editing an existing module (selected row), upload immediately.
    if (selectedModuleId && token) {
      uploadIcon(
        apiBase,
        token,
        selectedModuleId,
        file,
        setStatus,
        setIconPreview,
        setNewCustomIcon,
        () => fetchModules(apiBase, token, setModules, setStatus, setLoading)
      );
      return;
    }

    // Buffer for new module creation.
    const url = URL.createObjectURL(file);
    setPendingIconFile(file);
    setPendingIconPreview(url);
    setNewCustomIcon(file.name);
    setIconPreview(url);
    setNewIcon('custom');
  };

  const handleHeroFileSelected = (file: File) => {
    if (!file) return;
    if (selectedModuleId && token) {
      // Validate client-side before upload.
      const extCheck = file.name.toLowerCase().endsWith('.png');
      if (!extCheck || file.size > 1024 * 1024) {
        setStatus('Hero image must be PNG and <= 1024 KB.');
        return;
      }
      const urlTemp = URL.createObjectURL(file);
      const imgTemp = new Image();
      imgTemp.onload = async () => {
        const { width, height } = imgTemp;
        const aspect = width / height;
        const diff = Math.abs(aspect - 2.0) / 2.0;
        if (width < 364 || height < 180 || width > 728 || height > 360 || diff > 0.03) {
          setStatus(`Hero image must be 2:1 (~3% tolerance) between 364x180 and 728x360. Got ${width}x${height}.`);
          URL.revokeObjectURL(urlTemp);
          return;
        }
        await uploadHero(apiBase, token, selectedModuleId, file, setStatus, setPendingHeroPreview, setHeroMeta, () =>
          fetchModules(apiBase, token, setModules, setStatus, setLoading)
        );
        URL.revokeObjectURL(urlTemp);
      };
      imgTemp.onerror = () => {
        setStatus('Invalid image file.');
        URL.revokeObjectURL(urlTemp);
      };
      imgTemp.src = urlTemp;
      return;
    }

    const ext = file.name.toLowerCase();
    if (!ext.endsWith('.png')) {
      setStatus('Hero image must be a PNG file.');
      return;
    }
    if (file.size > 1024 * 1024) {
      setStatus('Hero image exceeds 1024 KB.');
      return;
    }

    const url = URL.createObjectURL(file);
    const img = new Image();
    img.onload = () => {
      const { width, height } = img;
      const aspect = width / height;
      const diff = Math.abs(aspect - 2.0) / 2.0;
      if (width < 364 || height < 180 || width > 728 || height > 360 || diff > 0.03) {
        setStatus(`Hero image must be 2:1 (~3% tolerance) between 364x180 and 728x360. Got ${width}x${height}.`);
        URL.revokeObjectURL(url);
        return;
      }
      setPendingHeroFile(file);
      setPendingHeroPreview(url);
      const kb = (file.size / 1024).toFixed(1);
      setHeroMeta(`${width}x${height}, ${kb} KB`);
      setStatus('Hero image selected.');
      setNewIcon('none');
      setNewCustomIcon('');
      setPendingIconFile(null);
      setPendingIconPreview(null);
      setIconPreview(null);
    };
    img.onerror = () => {
      setStatus('Invalid image file.');
      URL.revokeObjectURL(url);
    };
    img.src = url;
  };

  const resetForm = (typeToKeep?: 'Standard' | 'Conditional' | 'Dynamic' | 'Hero') => {
    setNewName('');
    setNewModuleId(generateModuleId(''));
    setNewTitle('');
    setNewMessage('');
    setNewCategory('GeneralInfo');
    setNewType(typeToKeep ?? newType);
    setNewLinkUrl('https://');
    setNewSchedule('');
    setNewExpires('');
    setNewReminderHours('1');
    setNewIcon('info.ico');
    setNewCustomIcon('info.ico');
    setSoundEnabled(true);
    setPendingIconFile(null);
    setPendingIconPreview(null);
    setIconPreview('/icons/emblems/info.ico');
    setPendingHeroFile(null);
    setPendingHeroPreview(null);
    setHeroMeta('');
    setConditionalInterval('60');
    setNewConditionalScript('');
    setNewDynamicScript('');
    setDynamicMaxLength('240');
    setDynamicTrimWhitespace(true);
    setDynamicFailIfEmpty(true);
    setDynamicFallbackMessage('');
    setSelectedIds(new Set());
  };

  useEffect(() => {
    if (selectedModuleId) {
      const mod = modules.find((m) => m.id === selectedModuleId);
      if (mod?.iconFileName) {
        setNewCustomIcon(mod.iconFileName);
        setIconPreview(`${apiBase}/api/modules/${selectedModuleId}/icon?t=${Date.now()}`);
        setPendingIconFile(null);
        setPendingIconPreview(null);
        setPendingHeroFile(null);
        setPendingHeroPreview(null);
        setHeroMeta('');
      } else {
        setNewCustomIcon('');
        setIconPreview(null);
      }
      if (mod?.heroFileName) {
        setPendingHeroFile(null);
        const preview = `${apiBase}/api/modules/${selectedModuleId}/hero?t=${Date.now()}`;
        setPendingHeroPreview(preview);
        setHeroMeta('');
      }
    } else {
      setNewCustomIcon('');
      setIconPreview(null);
      setPendingHeroFile(null);
      setPendingHeroPreview(null);
      setHeroMeta('');
    }
  }, [selectedModuleId, modules, apiBase]);

  return (
    <div className={`page ${authed ? '' : 'unauth'} ${darkMode ? 'dark' : ''}`}>
      <nav className="top-nav">
        <div className="brand">Windows Notifier Cloud Admin</div>
        <div className="tabs type-tabs">
          {['Standard', 'Conditional', 'Dynamic', 'Hero'].map((t) => (
            <span
              key={t}
              className={`tab ${newType === t ? 'active' : ''}`}
              onClick={() => {
                setNewType(t as 'Standard' | 'Conditional' | 'Dynamic' | 'Hero');
                setStatus(`Switched to ${t} notification`);
              }}
            >
              {t} Notification
            </span>
          ))}
        </div>
        <div className="actions">
          <button onClick={() => setDarkMode((d) => !d)}>{darkMode ? 'Light mode' : 'Dark mode'}</button>
        </div>
      </nav>

      {!authed && (
        <section className="grid two">
          <div className="card stack">
            <div className="card-head">
              <div>
                <h3>Login Panel</h3>
                <p>Connect to your API and authenticate.</p>
              </div>
            </div>
            <div className="row">
              <label>API base</label>
              <input value={apiBase} onChange={(e) => setApiBase(e.target.value)} />
            </div>
            <div className="row">
              <label>Username</label>
              <input value={username} onChange={(e) => setUsername(e.target.value)} />
            </div>
            <div className="row">
              <label>Password</label>
              <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} />
            </div>
            <div className="actions">
              <button
                className="btn primary"
                onClick={() => handleLogin(apiBase, username, password, setToken, setStatus, setLoading, setModules)}
                disabled={loading}
              >
                Login (DevLocal)
              </button>
            </div>
            <div className={`status ${status ? 'visible' : ''}`}>{status}</div>
            <small className="hint">
              API base auto-fills from the last value. Current default is {apiBase}. Ensure the API is running on that host/port.
            </small>
          </div>
        </section>
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

          <section className="card stack">
            <div className="card-head">
              <div>
                <h3>Modules</h3>
                <p>Create, search, and export modules.</p>
              </div>
              <div className="actions">
                <button
                  className="btn primary ghost"
                  onClick={() => {
                    setSelectedIds(new Set());
                    resetForm(newType);
                  }}
                  disabled={loading}
                >
                  New
                </button>
                <button className="btn primary" onClick={handleSaveDraft} disabled={loading || !token}>
                  Save
                </button>
              </div>
            </div>

        <ModuleForm
          newName={newName}
          setNewName={setNewName}
          newModuleId={newModuleId}
          newTitle={newTitle}
          setNewTitle={setNewTitle}
          setNewModuleId={setNewModuleId}
          newMessage={newMessage}
          setNewMessage={setNewMessage}
          newCategory={newCategory}
          setNewCategory={setNewCategory}
          newType={newType}
          setNewType={setNewType}
          newLinkUrl={newLinkUrl}
          setNewLinkUrl={setNewLinkUrl}
          newSchedule={newSchedule}
          setNewSchedule={setNewSchedule}
          newExpires={newExpires}
          setNewExpires={setNewExpires}
          newReminderHours={newReminderHours}
          setNewReminderHours={setNewReminderHours}
          newIcon={newIcon}
          setNewIcon={setNewIcon}
          newCustomIcon={newCustomIcon}
          setNewCustomIcon={setNewCustomIcon}
          soundEnabled={soundEnabled}
          setSoundEnabled={setSoundEnabled}
          showIconPicker={showIconPicker}
          setShowIconPicker={setShowIconPicker}
          iconPreview={iconPreview}
          setIconPreview={setIconPreview}
          selectedModuleId={selectedModuleId}
          onPickIconFile={(file) => handleIconFileSelected(file)}
          pendingIconPreview={pendingIconPreview}
          onPickHeroFile={(file) => handleHeroFileSelected(file)}
          pendingHeroPreview={pendingHeroPreview}
          heroMeta={heroMeta}
          onSaveDraft={handleSaveDraft}
          onExport={handleExportSelected}
          disableSave={!token || loading}
          disableExport={!token || loading || !selectedModuleId}
          newConditionalScript={newConditionalScript}
          setNewConditionalScript={setNewConditionalScript}
          newDynamicScript={newDynamicScript}
          setNewDynamicScript={setNewDynamicScript}
          dynamicMaxLength={dynamicMaxLength}
          setDynamicMaxLength={setDynamicMaxLength}
          dynamicTrimWhitespace={dynamicTrimWhitespace}
          setDynamicTrimWhitespace={setDynamicTrimWhitespace}
          dynamicFailIfEmpty={dynamicFailIfEmpty}
          setDynamicFailIfEmpty={setDynamicFailIfEmpty}
          dynamicFallbackMessage={dynamicFallbackMessage}
          setDynamicFallbackMessage={setDynamicFallbackMessage}
          conditionalInterval={conditionalInterval}
          setConditionalInterval={setConditionalInterval}
        />

        <div className="filters-row">
          <div className="filters">
            <input
              className="search"
              placeholder="Search by name, id, or campaign"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
            />
            <div className="chips">
              {['All', 'GeneralInfo', 'Security', 'Compliance', 'Maintenance', 'Application'].map((c) => (
                <span key={c} className={`chip ${categoryFilter === c ? 'active' : ''}`} onClick={() => setCategoryFilter(c)}>
                  {c === 'All' ? 'All Categories' : c.replace('Info', '')}
                </span>
              ))}
            </div>
          </div>
          <div className="filters-actions">
            <span className="actions-placeholder"></span>
            <button onClick={handleExportSelected} disabled={!token || loading || !selectedModuleId}>
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
                <th>Campaign</th>
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
                  <td>{m.campaign ?? '—'}</td>
                  <td>{m.version}</td>
                  <td>
                    <span className={`badge ${m.isPublished ? 'success' : 'muted'}`}>
                      {m.isPublished ? 'Published' : 'Draft'}
                    </span>
                  </td>
                  <td className="row-actions compact">
                    <button className="icon-btn" title="Edit (placeholder)" disabled>
                      ✎
                    </button>
                    <button
                      className="icon-btn"
                      title="Export to Dev Core"
                      onClick={() => exportDevCore(apiBase, token, m.id, setStatus, setLoading)}
                      disabled={!token || loading || (m.type !== 'Standard' && m.type !== 'Hero')}
                    >
                      ⬆
                    </button>
                    <button
                      className="icon-btn"
                      title="Export Zip"
                      onClick={() => exportZip(apiBase, token, m.id, setStatus, setLoading)}
                      disabled={!token || loading}
                    >
                      ⬇
                    </button>
                    <button className="icon-btn" title="Metrics (placeholder)" disabled>
                      📊
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
          {filteredModules.length === 0 && <div className="empty">No modules match your filters.</div>}
        </div>
        <div className="table-actions">
          <button onClick={() => fetchModules(apiBase, token, setModules, setStatus, setLoading)} disabled={loading}>
            Refresh modules
          </button>
        </div>

        <div className="bulk-bar align-right">
          <div className="bulk-actions">
            <button onClick={() => setSelectedIds(new Set(filteredModules.map((m) => m.id)))} disabled={filteredModules.length === 0}>
              Check All
            </button>
            <button onClick={() => setSelectedIds(new Set())} disabled={selectedIds.size === 0}>
              Uncheck All
            </button>
            <button
              onClick={() =>
                removeSelected(
                  apiBase,
                  token,
                  selectedIds,
                  setStatus,
                  setLoading,
                  () => fetchModules(apiBase, token, setModules, setStatus, setLoading),
                  setSelectedIds
                )
              }
              disabled={!token || loading || selectedIds.size === 0}
            >
              Remove checked
            </button>
          </div>
          <div className="bulk-status">Selected: {selectedIds.size}</div>
        </div>

          </section>
          <section className="card stack">
            <div className="card-head">
              <div>
                <h3>Telemetry Overview</h3>
                <p>Quick glance at recent interactions.</p>
              </div>
              <div className="actions">
                <button onClick={() => fetchTelemetry(apiBase, token, setTelemetry, setTelemetryModules, setStatus, setLoading)} disabled={loading}>
                  Refresh telemetry
                </button>
                <button disabled={!telemetry} onClick={() => downloadTelemetry(telemetry)} title="Download telemetry JSON">
                  Download JSON
                </button>
              </div>
            </div>
            {telemetry ? (
              <>
                <div className="telemetry-grid">
                  <MetricTile label="Toasts Shown" value={telemetry?.toastShown ?? '—'} />
                  <MetricTile label="Button OK" value={telemetry?.buttonOk ?? '—'} />
                  <MetricTile label="Learn More" value={telemetry?.buttonMoreInfo ?? '—'} />
                  <MetricTile label="Completed" value={telemetry?.completed ?? '—'} />
                  <MetricTile label="First Seen" value={formatDate(telemetry?.rangeStartUtc)} small />
                  <MetricTile label="Last Seen" value={formatDate(telemetry?.rangeEndUtc)} small />
                </div>
                <div className="chart-placeholder">Trend chart placeholder</div>
                <div className="table-actions spaced-top">
                  <h4>Per-module telemetry</h4>
                  <div className="module-telemetry-table">
                    <table>
                      <thead>
                        <tr>
                          <th>Module</th>
                          <th>Name</th>
                          <th>Type</th>
                          <th>Shown</th>
                          <th>OK</th>
                          <th>Learn</th>
                          <th>Completed</th>
                          <th>First Seen</th>
                          <th>Last Seen</th>
                        </tr>
                      </thead>
                      <tbody>
                        {telemetryModules.length === 0 && (
                          <tr>
                            <td colSpan={9} className="empty">No telemetry yet.</td>
                          </tr>
                        )}
                        {telemetryModules.map((m) => (
                          <tr key={m.moduleId}>
                            <td>{m.moduleId}</td>
                            <td>{m.displayName ?? '—'}</td>
                            <td>{m.type ?? '—'}</td>
                            <td>{m.toastShown ?? 0}</td>
                            <td>{m.buttonOk ?? 0}</td>
                            <td>{m.buttonMoreInfo ?? 0}</td>
                            <td>{m.completed ?? 0}</td>
                            <td>{formatDate(m.firstSeen)}</td>
                            <td>{formatDate(m.lastSeen)}</td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                </div>
              </>
            ) : (
              <div className="empty">Telemetry not loaded. Click Refresh telemetry to fetch.</div>
            )}
          </section>
          <IconPickerModal
            isOpen={showIconPicker}
            onClose={() => setShowIconPicker(false)}
            onUpload={(file) =>
              handleIconFileSelected(file)
            }
          />
        </>
      )}
    </div>
  );
}

function IconPickerModal(props: {
  isOpen: boolean;
  onClose: () => void;
  onUpload: (file: File) => void;
}) {
  if (!props.isOpen) return null;

  const handleFile = (file: File | undefined | null) => {
    if (!file) return;
    props.onUpload(file);
    props.onClose();
  };

  return (
    <input
      type="file"
      accept=".ico,.png,.jpg,.jpeg"
      onChange={(e) => handleFile(e.target.files?.[0])}
      style={{ display: 'none' }}
    />
  );
}

function ModuleForm(props: {
  newName: string;
  setNewName: (s: string) => void;
  newModuleId: string;
  newTitle: string;
  setNewTitle: (s: string) => void;
  setNewModuleId: (s: string) => void;
  newMessage: string;
  setNewMessage: (s: string) => void;
  newCategory: string;
  setNewCategory: (s: string) => void;
  newType: 'Standard' | 'Conditional' | 'Dynamic' | 'Hero';
  setNewType: (s: 'Standard' | 'Conditional' | 'Dynamic' | 'Hero') => void;
  newLinkUrl: string;
  setNewLinkUrl: (s: string) => void;
  newSchedule: string;
  setNewSchedule: (s: string) => void;
  newExpires: string;
  setNewExpires: (s: string) => void;
  newReminderHours: string;
  setNewReminderHours: (s: string) => void;
  newIcon: string;
  setNewIcon: (s: string) => void;
  newCustomIcon: string;
  setNewCustomIcon: (s: string) => void;
  soundEnabled: boolean;
  setSoundEnabled: (b: boolean) => void;
  showIconPicker: boolean;
  setShowIconPicker: (b: boolean) => void;
  iconPreview: string | null;
  setIconPreview: (s: string | null) => void;
  selectedModuleId: string | null;
  onPickIconFile: (file: File) => void;
  pendingIconPreview: string | null;
  onPickHeroFile: (file: File) => void;
  pendingHeroPreview: string | null;
  heroMeta: string;
  onSaveDraft: () => void;
  onExport: () => void;
  onPublish?: () => void;
  disableSave?: boolean;
  disableExport?: boolean;
  newConditionalScript: string;
  setNewConditionalScript: (s: string) => void;
  newDynamicScript: string;
  setNewDynamicScript: (s: string) => void;
  dynamicMaxLength: string;
  setDynamicMaxLength: (s: string) => void;
  dynamicTrimWhitespace: boolean;
  setDynamicTrimWhitespace: (b: boolean) => void;
  dynamicFailIfEmpty: boolean;
  setDynamicFailIfEmpty: (b: boolean) => void;
  dynamicFallbackMessage: string;
  setDynamicFallbackMessage: (s: string) => void;
  conditionalInterval: string;
  setConditionalInterval: (s: string) => void;
}) {
  return (
    <div className="editor">
      <div className="editor-grid">
        <div className="form-card">
          <div className="section-head">
            <h4>General</h4>
          </div>
          <div className="stack">
            <label>
              Campaign
              <input
                placeholder="Campaign name"
                value={props.newName}
                onChange={(e) => props.setNewName(e.target.value)}
              />
            </label>
            <label>
              Module ID
              <input value={props.newModuleId} readOnly title="Auto-generated from title" />
            </label>
            <label>
              Category
              <select value={props.newCategory} onChange={(e) => props.setNewCategory(e.target.value)}>
                <option value="GeneralInfo">General</option>
                <option value="Security">Security</option>
                <option value="Compliance">Compliance</option>
                <option value="Maintenance">Maintenance</option>
                <option value="Application">Application</option>
              </select>
            </label>
            <label>
              Title
              <input
                placeholder="Notification title"
                maxLength={60}
                value={props.newTitle}
                onChange={(e) => {
                  const title = e.target.value.slice(0, 60);
                  props.setNewTitle(title);
                  props.setNewModuleId(generateModuleId(title));
                }}
              />
            </label>
            {props.newType !== 'Hero' && props.newType !== 'Dynamic' && (
              <label>
                Message
                <textarea
                  placeholder="Notification message (160 chars)"
                  maxLength={160}
                  value={props.newMessage}
                  onChange={(e) => props.setNewMessage(e.target.value)}
                  disabled={props.newType === 'Dynamic'}
                />
              </label>
            )}
          </div>
        </div>

        <div className="form-card">
          <div className="section-head">
            <h4>Schedule & Media</h4>
          </div>
          <div className="stack gap-12">
            <div className="stack">
              <label>
                Schedule (UTC)
                <input type="datetime-local" value={props.newSchedule} onChange={(e) => props.setNewSchedule(e.target.value)} />
              </label>
              <label>
                Expires (UTC)
                <input type="datetime-local" value={props.newExpires} onChange={(e) => props.setNewExpires(e.target.value)} />
              </label>
            </div>

            <div className="media-row">
              {props.newType === 'Hero' ? (
                <div className="icon-block">
                  <label>Hero image upload</label>
                  <div className="file-row">
                    <input
                      type="file"
                      accept=".png"
                      style={{ display: 'none' }}
                      id="heroFileInput"
                      onChange={(e) => {
                        const file = e.target.files?.[0];
                        if (file) props.onPickHeroFile(file);
                        e.target.value = '';
                      }}
                    />
                    <button type="button" onClick={() => document.getElementById('heroFileInput')?.click()}>
                      Browse…
                    </button>
                    <span className="file-name">{props.heroMeta || 'Select a 2:1 PNG banner'}</span>
                  </div>
                  <small className="hint">PNG only. 364x180 to 728x360. Max 1024 KB. Stored as hero.png.</small>
                  {props.pendingHeroPreview && (
                    <div className="icon-preview side">
                      <img src={props.pendingHeroPreview} alt="Hero preview" />
                    </div>
                  )}
                </div>
              ) : (
                <>
                  <div className="icon-block">
                    <label>Icon upload</label>
                    <div className="file-row">
                      <input
                        type="file"
                        accept=".ico,.png,.jpg,.jpeg"
                        style={{ display: 'none' }}
                        id="iconFileInput"
                        onChange={(e) => {
                          const file = e.target.files?.[0];
                          if (file) props.onPickIconFile(file);
                          e.target.value = '';
                        }}
                      />
                      <button type="button" onClick={() => document.getElementById('iconFileInput')?.click()}>
                        Browse…
                      </button>
                      <span className="file-name">{props.newCustomIcon || 'Default (info.ico)'}</span>
                    </div>
                    <small className="hint">Icons are stored server-side under module-assets. Supports .ico, .png, .jpg.</small>
                  </div>

                  {(props.pendingIconPreview || props.iconPreview) && (
                    <div className="icon-preview side">
                      <img
                        src={props.pendingIconPreview || props.iconPreview || ''}
                        alt="Icon preview"
                        onError={() => props.setIconPreview(null)}
                      />
                    </div>
                  )}
                </>
              )}

              <div className="reminder-block">
                <label>
                  Reminder hours
                  <input
                    type="number"
                    min="0"
                    placeholder="1"
                    value={props.newReminderHours}
                    onChange={(e) => props.setNewReminderHours(e.target.value)}
                    className="narrow-input"
                  />
                </label>
              </div>
            </div>
          </div>

          <label>
            Primary link (optional)
            <input
              placeholder="https:// | file:// | \\\\share\\path"
              value={props.newLinkUrl}
              onChange={(e) => props.setNewLinkUrl(e.target.value)}
            />
          </label>
          <label className="checkbox-row left-align">
            <input
              type="checkbox"
              checked={props.soundEnabled}
              onChange={(e) => props.setSoundEnabled(e.target.checked)}
            />
            <span>Play notification sound</span>
          </label>
          <small className="hint">If no scheme is provided, the core will attempt to normalize.</small>
        </div>
      </div>

      {props.newType === 'Conditional' && (
        <div className="form-card">
          <div className="section-head">
            <h4>Behavior</h4>
            <p className="hint">Runs a conditional PowerShell script; toast shows only if script Exit 0.</p>
          </div>
          <div className="stack two">
            <label>
              Check interval (minutes)
              <input
                type="number"
                min="1"
                value={props.conditionalInterval}
                onChange={(e) => props.setConditionalInterval(e.target.value)}
                className="narrow-input"
              />
            </label>
          </div>
          <label>
            Conditional script (PowerShell)
            <textarea
              placeholder="Write or paste the conditional script..."
              value={props.newConditionalScript}
              onChange={(e) => props.setNewConditionalScript(e.target.value)}
              className="script-area"
            />
          </label>
        </div>
      )}

      {props.newType === 'Dynamic' && (
        <div className="form-card">
          <div className="section-head">
            <h4>Behavior</h4>
            <p className="hint">Runs a dynamic PowerShell script that returns title/message/link/icon.</p>
          </div>
          <label>
            Dynamic script (PowerShell)
            <textarea
              placeholder="Write or paste the dynamic script..."
              value={props.newDynamicScript}
              onChange={(e) => props.setNewDynamicScript(e.target.value)}
              className="script-area"
            />
          </label>
          <div className="stack two">
            <label>
              Max length
              <input
                type="number"
                min="1"
                value={props.dynamicMaxLength}
                onChange={(e) => props.setDynamicMaxLength(e.target.value)}
                className="narrow-input"
              />
            </label>
            <label className="checkbox-row left-align">
              <input
                type="checkbox"
                checked={props.dynamicTrimWhitespace}
                onChange={(e) => props.setDynamicTrimWhitespace(e.target.checked)}
              />
              <span>Trim whitespace</span>
            </label>
            <label className="checkbox-row left-align">
              <input
                type="checkbox"
                checked={props.dynamicFailIfEmpty}
                onChange={(e) => props.setDynamicFailIfEmpty(e.target.checked)}
              />
              <span>Fail if empty</span>
            </label>
            <label>
              Fallback message
              <input
                placeholder="Optional fallback message"
                value={props.dynamicFallbackMessage}
                onChange={(e) => props.setDynamicFallbackMessage(e.target.value)}
              />
            </label>
          </div>
        </div>
      )}

    </div>
  );
}

function MetricTile({ label, value, small }: { label: string; value: any; small?: boolean }) {
  return (
    <div className={`metric ${small ? 'small' : ''}`}>
      <div className="metric-label">{label}</div>
      <div className="metric-value">{value ?? '—'}</div>
    </div>
  );
}

function formatDate(val: any) {
  if (!val) return '—';
  const d = new Date(val);
  if (isNaN(d.getTime())) return '—';
  return d.toLocaleString(undefined, {
    year: 'numeric',
    month: 'short',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit'
  });
}

async function handleLogin(
  apiBase: string,
  username: string,
  password: string,
  setToken: (t: string | null) => void,
  setStatus: (s: string) => void,
  setLoading: (b: boolean) => void,
  setModules: (m: ModuleRow[]) => void
) {
  try {
    setLoading(true);
    setStatus('Logging in...');
    const res = await fetch(`${apiBase}/api/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ username, password })
    });
    if (!res.ok) {
      const txt = await res.text();
      setStatus(`Login failed (${res.status}): ${txt}`);
      setToken(null);
      return;
    }
    const data = await res.json();
    setToken(data.token);
    setStatus(`Logged in as ${username}`);
    await fetchModules(apiBase, data.token, setModules, setStatus, setLoading);
  } catch (err) {
    setStatus(`Login error: ${err}`);
    setToken(null);
  } finally {
    setLoading(false);
  }
}

async function fetchModules(
  apiBase: string,
  token: string | null,
  setModules: (m: ModuleRow[]) => void,
  setStatus: (s: string) => void,
  setLoading: (b: boolean) => void
) {
  if (!token) {
    setStatus('Login first.');
    return;
  }
  try {
    setLoading(true);
    setStatus('Loading modules...');
    const res = await fetch(`${apiBase}/api/modules`, {
      headers: { Authorization: `Bearer ${token}` }
    });
    if (!res.ok) {
      setStatus(`Load failed (${res.status})`);
      return;
    }
    const data = await res.json();
    setModules(data);
    setStatus(`Loaded ${data.length} modules.`);
  } catch (err) {
    setStatus(`Load error: ${err}`);
  } finally {
    setLoading(false);
  }
}

async function createModule(
  apiBase: string,
  token: string | null,
  name: string,
  moduleId: string,
  title: string,
  message: string,
  category: string,
  linkUrl: string,
  schedule: string,
  expires: string,
  reminderHours: string,
  setStatus: (s: string) => void,
  setModules: (m: ModuleRow[]) => void,
  setLoading: (b: boolean) => void,
  resetModuleId: (s: string) => void,
  iconChoice: string,
  pendingIconFile: File | null,
  pendingIconPreview: string | null,
  setPendingIconFile: (f: File | null) => void,
  setPendingIconPreview: (s: string | null) => void,
  setNewCustomIcon: (s: string) => void,
  setIconPreview: (s: string | null) => void,
  soundEnabled: boolean,
  moduleType: 'Standard' | 'Conditional' | 'Dynamic' | 'Hero',
  conditionalScript: string,
  dynamicScript: string,
  dynamicMaxLength: string,
  dynamicTrimWhitespace: boolean,
  dynamicFailIfEmpty: boolean,
  dynamicFallbackMessage: string,
  conditionalInterval: string,
  pendingHeroFile: File | null,
  pendingHeroPreview: string | null,
  setPendingHeroFile: (f: File | null) => void,
  setPendingHeroPreview: (s: string | null) => void,
  setHeroMeta: (s: string) => void
) {
  if (!token) {
    setStatus('Login first.');
    return;
  }
  try {
    setLoading(true);
    setStatus('Creating module...');
    const scheduleUtc = schedule ? new Date(schedule).toISOString() : null;
    const expiresUtc = expires ? new Date(expires).toISOString() : null;
    const resolvedIcon = moduleType === 'Hero'
      ? undefined
      : iconChoice === 'none'
        ? undefined
        : iconChoice === 'custom'
          ? pendingIconFile?.name
          : iconChoice;
    const payload = {
      displayName: name,
      moduleId,
      type: moduleType,
      category,
      description: '',
      title,
      message: moduleType === 'Hero' ? undefined : message,
      linkUrl,
      scheduleUtc,
      expiresUtc,
      reminderHours,
      iconFileName: resolvedIcon,
      soundEnabled,
      heroFileName: moduleType === 'Hero' && pendingHeroFile ? 'hero.png' : undefined,
      heroOriginalName: moduleType === 'Hero' && pendingHeroFile ? pendingHeroFile.name : undefined,
      conditionalScriptBody: moduleType === 'Conditional' ? conditionalScript || null : null,
      conditionalIntervalMinutes: moduleType === 'Conditional' && conditionalInterval ? Number(conditionalInterval) : null,
      dynamicScriptBody: moduleType === 'Dynamic' ? dynamicScript || null : null,
      dynamicMaxLength: moduleType === 'Dynamic' && dynamicMaxLength ? Number(dynamicMaxLength) : null,
      dynamicTrimWhitespace: moduleType === 'Dynamic' ? dynamicTrimWhitespace : null,
      dynamicFailIfEmpty: moduleType === 'Dynamic' ? dynamicFailIfEmpty : null,
      dynamicFallbackMessage: moduleType === 'Dynamic' ? (dynamicFallbackMessage || null) : null
    };
    const res = await fetch(`${apiBase}/api/modules`, {
      method: 'POST',
      headers: {
        Authorization: `Bearer ${token}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(payload)
    });
    if (!res.ok) {
      const text = await res.text();
      setStatus(`Create failed (${res.status}): ${text}`);
      return;
    }
    const created = await res.json();
    const createdId: string | undefined = created?.id;
    setStatus('Created module.');

    // If we buffered files for a new module, upload them now.
    if (createdId) {
      if (moduleType === 'Hero') {
        if (!pendingHeroFile) {
          setStatus('Hero image missing; please select a PNG banner.');
          return;
        }
        await uploadHero(
          apiBase,
          token,
          createdId,
          pendingHeroFile,
          setStatus,
          setPendingHeroPreview,
          setHeroMeta,
          () => fetchModules(apiBase, token, setModules, setStatus, setLoading)
        );
        setPendingHeroFile(null);
        setPendingHeroPreview(null);
      }
      if (moduleType !== 'Hero' && pendingIconFile) {
        await uploadIcon(
          apiBase,
          token,
          createdId,
          pendingIconFile,
          setStatus,
          setIconPreview,
          setNewCustomIcon,
          () => fetchModules(apiBase, token, setModules, setStatus, setLoading)
        );
        setPendingIconFile(null);
        setPendingIconPreview(null);
      } else if (moduleType !== 'Hero' && iconChoice !== 'none' && iconChoice.endsWith('.ico')) {
        await uploadPresetIcon(
          apiBase,
          token,
          createdId,
          iconChoice,
          setStatus,
          setIconPreview,
          setNewCustomIcon,
          () => fetchModules(apiBase, token, setModules, setStatus, setLoading)
        );
      } else {
        await fetchModules(apiBase, token, setModules, setStatus, setLoading);
      }
    } else {
      await fetchModules(apiBase, token, setModules, setStatus, setLoading);
    }

    // Reset ID for next module
    resetModuleId(generateModuleId(title));
  } catch (err) {
    setStatus(`Create error: ${err}`);
  } finally {
    setLoading(false);
  }
}

async function exportDevCore(
  apiBase: string,
  token: string | null,
  id: string,
  setStatus: (s: string) => void,
  setLoading: (b: boolean) => void
) {
  if (!token) {
    setStatus('Login first.');
    return;
  }
  try {
    setLoading(true);
    setStatus('Exporting to Dev Core...');
    const res = await fetch(`${apiBase}/api/export/${id}/devcore`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${token}` }
    });
    if (!res.ok) {
      const text = await res.text();
      setStatus(`Export failed (${res.status}): ${text}`);
      return;
    }
    const data = await res.json();
    setStatus(`Exported to Dev Core at ${data.devCorePath ?? 'n/a'}`);
  } catch (err) {
    setStatus(`Export error: ${err}`);
  } finally {
    setLoading(false);
  }
}

async function exportZip(
  apiBase: string,
  token: string | null,
  id: string,
  setStatus: (s: string) => void,
  setLoading: (b: boolean) => void
) {
  if (!token) {
    setStatus('Login first.');
    return;
  }
  try {
    setLoading(true);
    setStatus('Creating zip...');
    const res = await fetch(`${apiBase}/api/export/${id}/package`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${token}` }
    });
    if (!res.ok) {
      const text = await res.text();
      setStatus(`Zip failed (${res.status}): ${text}`);
      return;
    }
    const data = await res.json();
    setStatus(`Zip created at ${data.package ?? data.path}`);
  } catch (err) {
    setStatus(`Zip error: ${err}`);
  } finally {
    setLoading(false);
  }
}

async function uploadIcon(
  apiBase: string,
  token: string | null,
  moduleId: string | null,
  file: File,
  setStatus: (s: string) => void,
  setIconPreview: (s: string | null) => void,
  setNewCustomIcon: (s: string) => void,
  refreshModules: () => void
) {
  if (!token || !moduleId) {
    setStatus('Select a single module row before uploading an icon.');
    return;
  }

  try {
    setStatus('Uploading icon...');
    const form = new FormData();
    form.append('file', file);
    const res = await fetch(`${apiBase}/api/modules/${moduleId}/icon`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${token}` },
      body: form
    });
    if (!res.ok) {
      const txt = await res.text();
      setStatus(`Icon upload failed (${res.status}): ${txt}`);
      return;
    }
    const data = await res.json();
    const preview = data.previewUrl ?? `${apiBase}/api/modules/${moduleId}/icon?t=${Date.now()}`;
    setNewCustomIcon(data.fileName ?? file.name);
    setIconPreview(preview);
    setStatus('Icon uploaded.');
    await refreshModules();
  } catch (err) {
    setStatus(`Icon upload error: ${err}`);
  }
}

async function uploadHero(
  apiBase: string,
  token: string | null,
  moduleId: string | null,
  file: File,
  setStatus: (s: string) => void,
  setHeroPreview: (s: string | null) => void,
  setHeroMeta: (s: string) => void,
  refreshModules: () => void
) {
  if (!token || !moduleId) {
    setStatus('Select a single module row before uploading a hero image.');
    return;
  }

  try {
    setStatus('Uploading hero image...');
    const form = new FormData();
    form.append('file', file);
    const res = await fetch(`${apiBase}/api/modules/${moduleId}/hero`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${token}` },
      body: form
    });
    if (!res.ok) {
      const txt = await res.text();
      setStatus(`Hero upload failed (${res.status}): ${txt}`);
      return;
    }
    const data = await res.json();
    const preview = data.previewUrl ?? `${apiBase}/api/modules/${moduleId}/hero?t=${Date.now()}`;
    setHeroPreview(preview);
    setHeroMeta(file ? `${file.name}` : '');
    setStatus('Hero image uploaded.');
    await refreshModules();
  } catch (err) {
    setStatus(`Hero upload error: ${err}`);
  }
}

async function uploadPresetIcon(
  apiBase: string,
  token: string | null,
  moduleId: string | null,
  fileName: string,
  setStatus: (s: string) => void,
  setIconPreview: (s: string | null) => void,
  setNewCustomIcon: (s: string) => void,
  refreshModules: () => void
) {
  if (!token || !moduleId) {
    setStatus('Cannot upload icon: missing module or token.');
    return;
  }

  try {
    setStatus('Uploading preset icon...');
    const resp = await fetch(`/icons/emblems/${fileName}`);
    if (!resp.ok) {
      setStatus(`Preset icon not found: ${fileName}`);
      return;
    }
    const blob = await resp.blob();
    const file = new File([blob], fileName, { type: blob.type || 'image/x-icon' });
    await uploadIcon(apiBase, token, moduleId, file, setStatus, setIconPreview, setNewCustomIcon, refreshModules);
  } catch (err) {
    setStatus(`Preset icon upload error: ${err}`);
  }
}

async function removeSelected(
  apiBase: string,
  token: string | null,
  selected: Set<string>,
  setStatus: (s: string) => void,
  setLoading: (b: boolean) => void,
  refresh: () => void,
  setSelectedIds: (s: Set<string>) => void
) {
  if (!token || selected.size === 0) return;
  try {
    setLoading(true);
    setStatus('Removing selected modules...');
    for (const id of selected) {
      await fetch(`${apiBase}/api/modules/${id}`, {
        method: 'DELETE',
        headers: { Authorization: `Bearer ${token}` }
      });
    }
    setStatus('Removed selected modules.');
    setSelectedIds(new Set());
    refresh();
  } catch (err) {
    setStatus(`Remove failed: ${err}`);
  } finally {
    setLoading(false);
  }
}

async function fetchTelemetry(
  apiBase: string,
  token: string | null,
  setTelemetry: (s: any) => void,
  setTelemetryModules: (s: any[]) => void,
  setStatus: (s: string) => void,
  setLoading: (b: boolean) => void
) {
  if (!token) {
    setStatus('Login first.');
    return;
  }
  try {
    setLoading(true);
    setStatus('Loading telemetry...');
    const res = await fetch(`${apiBase}/api/reporting/summary`, {
      headers: { Authorization: `Bearer ${token}` }
    });
    if (!res.ok) {
      const txt = await res.text();
      setStatus(`Telemetry failed (${res.status}): ${txt}`);
      return;
    }
    const data = await res.json();

    // Also fetch per-module telemetry
    const resModules = await fetch(`${apiBase}/api/reporting/modules`, {
      headers: { Authorization: `Bearer ${token}` }
    });
    let modulesData: any[] = [];
    if (resModules.ok) {
      modulesData = await resModules.json();
    }

    setTelemetry(data);
    setTelemetryModules(modulesData);
    setStatus('Telemetry loaded.');
  } catch (err) {
    setStatus(`Telemetry error: ${err}`);
  } finally {
    setLoading(false);
  }
}

function generateModuleId(title: string): string {
  const rand = Math.floor(10000 + Math.random() * 90000);
  const safe = title
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/^-+|-+$/g, '');
  const tail = safe || 'module';
  return `module-${rand}-${tail}`;
}

function downloadTelemetry(obj: any) {
  const blob = new Blob([JSON.stringify(obj, null, 2)], { type: 'application/json' });
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = 'telemetry-summary.json';
  a.click();
  URL.revokeObjectURL(url);
}
