import React from 'react';
import type { NotificationType } from '../../types';
import { StandardFields } from '../notificationTypes/StandardFields';
import { ConditionalFields } from '../notificationTypes/ConditionalFields';
import { DynamicFields } from '../notificationTypes/DynamicFields';
import { HeroFields } from '../notificationTypes/HeroFields';

type FormState = {
  displayName: string;
  moduleId: string;
  title: string;
  message: string;
  category: string;
  type: NotificationType;
  linkUrl: string;
  schedule: string;
  expires: string;
  reminderHours: string;
  icon: string;
  customIcon: string;
  soundEnabled: boolean;
  conditionalScript: string;
  conditionalInterval: string;
  dynamicScript: string;
  dynamicMaxLength: string;
  dynamicTrimWhitespace: boolean;
  dynamicFailIfEmpty: boolean;
  dynamicFallbackMessage: string;
  coreEnabled: boolean;
  coreAutoClear: boolean;
  corePolling: string;
  coreHeartbeat: string;
  coreSound: boolean;
  coreExitVisible: boolean;
  coreStartStopVisible: boolean;
};

type Props = {
  formType: NotificationType;
  setFormType: (t: NotificationType) => void;
  formState: FormState;
  setFormState: (s: FormState) => void;
  onSave: () => void;
  onNew: () => void;
  disableSave?: boolean;
  onOpenTemplates: () => void;
  onIconFileSelected: (f: File) => void;
  onHeroFileSelected: (f: File) => void;
  iconPreviewUrl?: string | null;
  heroPreviewUrl?: string | null;
};

export const ModuleForm: React.FC<Props> = ({
  formType,
  setFormType,
  formState,
  setFormState,
  onSave,
  onNew,
  disableSave,
  onOpenTemplates,
  onIconFileSelected,
  onHeroFileSelected,
  iconPreviewUrl,
  heroPreviewUrl
}) => {
  const update = <K extends keyof FormState>(key: K, value: FormState[K]) => {
    setFormState({ ...formState, [key]: value });
  };

  const hiddenIconInputRef = React.useRef<HTMLInputElement | null>(null);

  const handleBrowseIcon = () => {
    hiddenIconInputRef.current?.click();
  };

  const handleIconChosen = (e: React.ChangeEvent<HTMLInputElement>) => {
    const f = e.target.files?.[0];
    if (f) {
      if (formType === 'Hero') {
        onHeroFileSelected(f);
      } else {
        update('customIcon', f.name);
        update('icon', f.name);
        onIconFileSelected(f);
      }
    }
  };

  const scriptCardVisible = formType === 'Conditional' || formType === 'Dynamic';

  return (
    <div className="editor">
      <div className="card-head">
        <div>
          <h3>Modules</h3>
          <p>Create, search, and export modules.</p>
        </div>
        <div className="actions">
          <button className="btn primary ghost" onClick={onNew}>
            New
          </button>
          <button className="btn primary" onClick={onSave} disabled={disableSave}>
            Save
          </button>
        </div>
      </div>

      <div className="tabs type-tabs">
        {(['Standard', 'Conditional', 'Dynamic', 'Hero', 'CoreSettings'] as NotificationType[]).map((t) => (
          <span
            key={t}
            className={`tab ${formType === t ? 'active' : ''}`}
            onClick={() => setFormType(t)}
          >
            {t === 'CoreSettings' ? 'Core Settings' : `${t} Notification`}
          </span>
        ))}
      </div>

      <div className="editor-grid">
        <div className="form-card span-3-5">
          <div className="section-head">
            <h4>General</h4>
          </div>
          <div className="stack">
            <label>
              Name
              <input
                placeholder="Module name"
                value={formState.displayName}
                onChange={(e) => update('displayName', e.target.value)}
              />
            </label>
            <label>
              Module ID
              <input value={formState.moduleId} readOnly title="Auto-generated from title" />
            </label>
            <label>
              Category
              <select
                value={formState.category}
                onChange={(e) => update('category', e.target.value)}
              >
                <option value="GeneralInfo">General</option>
                <option value="Security">Security</option>
                <option value="Compliance">Compliance</option>
                <option value="Maintenance">Maintenance</option>
                <option value="Application">Application</option>
              </select>
            </label>

            {formType === 'Standard' && (
              <StandardFields
                title={formState.title}
                setTitle={(v) => update('title', v)}
                message={formState.message}
                setMessage={(v) => update('message', v)}
              />
            )}

            {formType === 'Hero' && (
              <HeroFields
                title={formState.title}
                setTitle={(v) => update('title', v)}
                message={formState.message}
                setMessage={(v) => update('message', v)}
              />
            )}

            {(formType === 'Conditional' || formType === 'Dynamic') && (
              <>
                <label>
                  Title
                  <input
                    placeholder="Notification title"
                    maxLength={60}
                    value={formState.title}
                    onChange={(e) => update('title', e.target.value.slice(0, 60))}
                  />
                </label>
                {formType === 'Conditional' && (
                  <label>
                    Message
                    <textarea
                      placeholder="Notification message (160 chars)"
                      maxLength={160}
                      value={formState.message}
                      onChange={(e) => update('message', e.target.value)}
                    />
                  </label>
                )}
              </>
            )}

            {formType === 'CoreSettings' && (
              <div className="stack gap-12">
                <label className="checkbox-row left-align inline-checkbox">
                  <input
                    type="checkbox"
                    checked={formState.coreEnabled}
                    onChange={(e) => update('coreEnabled', e.target.checked)}
                  />
                  <span>Enable Core</span>
                </label>
                <label className="checkbox-row left-align inline-checkbox">
                  <input
                    type="checkbox"
                    checked={formState.coreSound}
                    onChange={(e) => update('coreSound', e.target.checked)}
                  />
                  <span>Allow notification sounds</span>
                </label>
                <label>
                  Polling interval (seconds)
                  <input
                    type="number"
                    min="60"
                    value={formState.corePolling}
                    onChange={(e) => update('corePolling', e.target.value)}
                  />
                </label>
                <label>
                  Heartbeat (seconds)
                  <input
                    type="number"
                    min="15"
                    value={formState.coreHeartbeat}
                    onChange={(e) => update('coreHeartbeat', e.target.value)}
                  />
                </label>
                <label className="checkbox-row left-align inline-checkbox">
                  <input
                    type="checkbox"
                    checked={formState.coreAutoClear}
                    onChange={(e) => update('coreAutoClear', e.target.checked)}
                  />
                  <span>Auto-clear completed modules</span>
                </label>
                <label className="checkbox-row left-align inline-checkbox">
                  <input
                    type="checkbox"
                    checked={formState.coreExitVisible}
                    onChange={(e) => update('coreExitVisible', e.target.checked)}
                  />
                  <span>Show Exit menu item</span>
                </label>
                <label className="checkbox-row left-align inline-checkbox">
                  <input
                    type="checkbox"
                    checked={formState.coreStartStopVisible}
                    onChange={(e) => update('coreStartStopVisible', e.target.checked)}
                  />
                  <span>Show Start/Stop menu items</span>
                </label>
              </div>
            )}
          </div>
        </div>
        <div className="spacer-card span-1-5" aria-hidden="true"></div>

        {formType !== 'CoreSettings' && (
          <div className="form-card">
          <div className="section-head">
            <h4>Schedule & Media</h4>
            </div>
            <div className="stack gap-12">
              <div className="stack">
                <label>
                  Schedule (UTC)
                  <input
                    type="datetime-local"
                    value={formState.schedule}
                    onChange={(e) => update('schedule', e.target.value)}
                  />
                </label>
                <label>
                  Expires (UTC)
                  <input
                    type="datetime-local"
                    value={formState.expires}
                    onChange={(e) => update('expires', e.target.value)}
                  />
                </label>
              </div>

              <div className="media-row">
                <div className="icon-block">
                  <label>{formType === 'Hero' ? 'Hero image (server-managed)' : 'Icon'}</label>
                  <div className="icon-actions">
                    <button type="button" className="btn ghost small" onClick={handleBrowseIcon}>
                      Browse
                    </button>
                    <input
                      ref={hiddenIconInputRef}
                      type="file"
                      accept={formType === 'Hero' ? '.png' : '.ico,.png,.jpg'}
                      style={{ display: 'none' }}
                      onChange={handleIconChosen}
                    />
                  </div>
                  {formType !== 'Hero' && iconPreviewUrl && (
                    <div className="icon-preview side">
                      <img src={iconPreviewUrl} alt="icon preview" height={40} />
                    </div>
                  )}
                  {formType === 'Hero' && heroPreviewUrl && (
                    <div className="icon-preview side">
                      <img src={heroPreviewUrl} alt="hero preview" height={70} />
                    </div>
                  )}
                  <small className="hint">
                    {formType === 'Hero'
                      ? 'Hero banner: PNG only, 2:1 ratio (e.g. 728x360), up to ~1MB.'
                      : 'File upload and preview are handled by the backend admin or a future media manager.'}
                  </small>
                </div>

                <div className="reminder-block">
                  <label>
                    Reminder hours
                    <input
                      type="number"
                      min="0"
                      placeholder="1"
                      value={formState.reminderHours}
                      onChange={(e) => update('reminderHours', e.target.value)}
                      className="narrow-input"
                    />
                  </label>
                </div>
              </div>
            </div>

            <label>
              Primary link (optional)
              <input
                placeholder="https:// | file:// | \\share\\path"
                value={formState.linkUrl}
                onChange={(e) => update('linkUrl', e.target.value)}
              />
            </label>
            <label className="checkbox-row left-align inline-checkbox">
              <input
                type="checkbox"
                checked={formState.soundEnabled}
                onChange={(e) => update('soundEnabled', e.target.checked)}
              />
              <span>Play notification sound</span>
            </label>
            <small className="hint">If no scheme is provided, the core will attempt to normalize.</small>
          </div>
        )}
      </div>

      {scriptCardVisible && (
        <div className="form-card full-width">
          <div className="section-head">
            <h4>Script logic</h4>
            <div className="actions">
              <button className="btn ghost" type="button" onClick={onOpenTemplates}>
                PowerShell Templates
              </button>
            </div>
          </div>
          <div className="stack">
            {formType === 'Conditional' && (
              <ConditionalFields
                title={formState.title}
                setTitle={(v) => update('title', v)}
                message={formState.message}
                setMessage={(v) => update('message', v)}
                conditionalInterval={formState.conditionalInterval}
                setConditionalInterval={(v) => update('conditionalInterval', v)}
                conditionalScript={formState.conditionalScript}
                setConditionalScript={(v) => update('conditionalScript', v)}
              />
            )}
            {formType === 'Dynamic' && (
              <DynamicFields
                title={formState.title}
                setTitle={(v) => update('title', v)}
                dynamicScript={formState.dynamicScript}
                setDynamicScript={(v) => update('dynamicScript', v)}
                dynamicMaxLength={formState.dynamicMaxLength}
                setDynamicMaxLength={(v) => update('dynamicMaxLength', v)}
                dynamicTrimWhitespace={formState.dynamicTrimWhitespace}
                setDynamicTrimWhitespace={(v) => update('dynamicTrimWhitespace', v)}
                dynamicFailIfEmpty={formState.dynamicFailIfEmpty}
                setDynamicFailIfEmpty={(v) => update('dynamicFailIfEmpty', v)}
                dynamicFallbackMessage={formState.dynamicFallbackMessage}
                setDynamicFallbackMessage={(v) => update('dynamicFallbackMessage', v)}
              />
            )}
            <small className="hint">Provide PowerShell that returns text for the toast body.</small>
          </div>
        </div>
      )}
    </div>
  );
};
