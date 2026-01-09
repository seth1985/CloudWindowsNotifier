import React from 'react';
import { Shield, Globe, Lock, Terminal, ChevronRight } from 'lucide-react';
import type { NotificationType } from '../../../types';
import { StandardFields } from '../notificationTypes/StandardFields';
import { ConditionalFields } from '../notificationTypes/ConditionalFields';
import { DynamicFields } from '../notificationTypes/DynamicFields';
import { HeroFields } from '../notificationTypes/HeroFields';
import { cn } from '../../../lib/utils';

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
  isAdvanced?: boolean;
  isAdmin?: boolean;
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
  heroPreviewUrl,
  isAdvanced = false,
  isAdmin = false
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
    <div className="max-w-5xl mx-auto space-y-8">
      <div className="flex gap-2 border-b border-border mb-8 pb-0 overflow-x-auto no-scrollbar">
        {(['Standard', 'Conditional', 'Dynamic', 'Hero', 'CoreSettings'] as NotificationType[])
          .filter(t => {
            if (t === 'CoreSettings') return isAdmin;
            if (t === 'Conditional' || t === 'Dynamic') return isAdvanced;
            return true;
          })
          .map((t) => (
            <button
              key={t}
              type="button"
              className={cn(
                "px-6 py-4 text-[10px] font-black uppercase tracking-[0.2em] border-b-[3px] transition-all whitespace-nowrap",
                formType === t
                  ? "border-primary-main text-primary-main"
                  : "border-transparent text-text-tertiary hover:text-text-primary hover:border-border"
              )}
              onClick={() => setFormType(t)}
            >
              {t === 'CoreSettings' ? 'Core Settings' : `${t} Notification`}
            </button>
          ))}
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-10">
        <div className="space-y-8">
          <div className="flex items-center gap-4 mb-2">
            <div className="p-2.5 bg-primary-main/10 text-primary-main rounded-xl border border-primary-main/20 shadow-sm">
              <Shield className="w-5 h-5" />
            </div>
            <h4 className="text-xl font-black text-text-primary tracking-tight uppercase">General Information</h4>
          </div>

          <div className="space-y-6">
            <div className="space-y-2.5">
              <label className="block text-[10px] font-black text-text-secondary uppercase tracking-[0.15em] ml-1">Module Name</label>
              <input
                className="input"
                placeholder="Enter module name"
                value={formState.displayName}
                onChange={(e) => update('displayName', e.target.value)}
              />
            </div>


            <div className="space-y-2.5">
              <label className="block text-[10px] font-black text-text-secondary uppercase tracking-[0.15em] ml-1">Functional Category</label>
              <div className="relative">
                <select
                  className="input appearance-none cursor-pointer pr-10"
                  value={formState.category}
                  onChange={(e) => update('category', e.target.value)}
                >
                  <option value="GeneralInfo">General</option>
                  <option value="Security">Security</option>
                  <option value="Compliance">Compliance</option>
                  <option value="Maintenance">Maintenance</option>
                  <option value="Application">Application</option>
                </select>
                <div className="absolute right-4 top-1/2 -translate-y-1/2 pointer-events-none text-text-tertiary">
                  <ChevronRight className="w-4 h-4 rotate-90" />
                </div>
              </div>
            </div>

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
                <div className="space-y-2.5">
                  <label className="block text-[10px] font-black text-text-secondary uppercase tracking-[0.15em] ml-1">Notification Title</label>
                  <input
                    className="input"
                    placeholder="Enter short title"
                    maxLength={60}
                    value={formState.title}
                    onChange={(e) => update('title', e.target.value.slice(0, 60))}
                  />
                </div>
                {formType === 'Conditional' && (
                  <div className="space-y-2.5">
                    <label className="block text-[10px] font-black text-text-secondary uppercase tracking-[0.15em] ml-1">Toast Message</label>
                    <textarea
                      className="input h-32 resize-none"
                      placeholder="Notification content (max 160 characters)"
                      maxLength={160}
                      value={formState.message}
                      onChange={(e) => update('message', e.target.value)}
                    />
                  </div>
                )}
              </>
            )}

            {formType === 'CoreSettings' && (
              <div className="space-y-6 pt-2">
                <div className="grid grid-cols-1 gap-4">
                  <label className="flex items-center gap-4 p-4 card shadow-sm cursor-pointer hover:border-primary-main/50 transition-all group">
                    <input
                      type="checkbox"
                      className="w-5 h-5 rounded border-border bg-input text-primary-main focus:ring-primary-main transition-all cursor-pointer"
                      checked={formState.coreEnabled}
                      onChange={(e) => update('coreEnabled', e.target.checked)}
                    />
                    <div className="flex flex-col">
                      <span className="text-sm font-bold text-text-primary">Enable Core Service</span>
                      <span className="text-[10px] text-text-tertiary uppercase font-bold tracking-tight">Allows background execution</span>
                    </div>
                  </label>
                  <label className="flex items-center gap-4 p-4 card shadow-sm cursor-pointer hover:border-primary-main/50 transition-all group">
                    <input
                      type="checkbox"
                      className="w-5 h-5 rounded border-border bg-input text-primary-main focus:ring-primary-main transition-all cursor-pointer"
                      checked={formState.coreSound}
                      onChange={(e) => update('coreSound', e.target.checked)}
                    />
                    <div className="flex flex-col">
                      <span className="text-sm font-bold text-text-primary">Master Audio Toggle</span>
                      <span className="text-[10px] text-text-tertiary uppercase font-bold tracking-tight">Audio feedback for all events</span>
                    </div>
                  </label>
                </div>

                <div className="grid grid-cols-2 gap-6">
                  <div className="space-y-2.5">
                    <label className="block text-[10px] font-black text-text-secondary uppercase tracking-[0.15em] ml-1">Polling Interval (s)</label>
                    <input
                      className="input h-11"
                      type="number"
                      min="60"
                      value={formState.corePolling}
                      onChange={(e) => update('corePolling', e.target.value)}
                    />
                  </div>
                  <div className="space-y-2.5">
                    <label className="block text-[10px] font-black text-text-secondary uppercase tracking-[0.15em] ml-1">Heartbeat Limit (s)</label>
                    <input
                      className="input h-11"
                      type="number"
                      min="15"
                      value={formState.coreHeartbeat}
                      onChange={(e) => update('coreHeartbeat', e.target.value)}
                    />
                  </div>
                </div>

                <div className="space-y-3 p-4 bg-surface-chip/20 rounded-xl border border-border/50">
                  <p className="text-[10px] font-black text-text-tertiary uppercase tracking-widest mb-2 px-1">Visibility Options</p>
                  <label className="flex items-center gap-3 cursor-pointer group px-1">
                    <input
                      type="checkbox"
                      className="w-4 h-4 rounded border-border bg-input text-primary-main focus:ring-primary-main"
                      checked={formState.coreAutoClear}
                      onChange={(e) => update('coreAutoClear', e.target.checked)}
                    />
                    <span className="text-xs font-bold text-text-secondary group-hover:text-text-primary transition-colors">Clear module after completion</span>
                  </label>
                  <label className="flex items-center gap-3 cursor-pointer group px-1">
                    <input
                      type="checkbox"
                      className="w-4 h-4 rounded border-border bg-input text-primary-main focus:ring-primary-main"
                      checked={formState.coreExitVisible}
                      onChange={(e) => update('coreExitVisible', e.target.checked)}
                    />
                    <span className="text-xs font-bold text-text-secondary group-hover:text-text-primary transition-colors">Show Exit in system tray</span>
                  </label>
                  <label className="flex items-center gap-3 cursor-pointer group px-1">
                    <input
                      type="checkbox"
                      className="w-4 h-4 rounded border-border bg-input text-primary-main focus:ring-primary-main"
                      checked={formState.coreStartStopVisible}
                      onChange={(e) => update('coreStartStopVisible', e.target.checked)}
                    />
                    <span className="text-xs font-bold text-text-secondary group-hover:text-text-primary transition-colors">Show Dynamic Control menu</span>
                  </label>
                </div>
              </div>
            )}
          </div>
        </div>

        {formType !== 'CoreSettings' && (
          <div className="space-y-8">
            <div className="flex items-center gap-4 mb-2">
              <div className="p-2.5 bg-purple-500/10 text-purple-600 dark:text-purple-400 rounded-xl border border-purple-500/20 shadow-sm">
                <Globe className="w-5 h-5" />
              </div>
              <h4 className="text-xl font-black text-text-primary tracking-tight uppercase">Schedule & Assets</h4>
            </div>

            <div className="space-y-8">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div className="space-y-2.5">
                  <label className="block text-[10px] font-black text-text-secondary uppercase tracking-[0.15em] ml-1">Schedule (UTC)</label>
                  <input
                    className="input h-11"
                    type="datetime-local"
                    value={formState.schedule}
                    onChange={(e) => update('schedule', e.target.value)}
                  />
                </div>
                <div className="space-y-2.5">
                  <label className="block text-[10px] font-black text-text-secondary uppercase tracking-[0.15em] ml-1">Expires (UTC)</label>
                  <input
                    className="input h-11"
                    type="datetime-local"
                    value={formState.expires}
                    onChange={(e) => update('expires', e.target.value)}
                  />
                </div>
              </div>

              <div className="flex flex-col md:flex-row gap-8 items-end">
                <div className="flex-1 space-y-4">
                  <label className="block text-[10px] font-black text-text-secondary uppercase tracking-[0.15em] ml-1">{formType === 'Hero' ? 'Hero Banner Image' : 'Digital Icon Asset'}</label>
                  <div className="flex gap-4 items-center">
                    <button
                      type="button"
                      className="btn btn-secondary h-11 px-6 text-xs"
                      onClick={handleBrowseIcon}
                    >
                      Choose File
                    </button>
                    <input
                      ref={hiddenIconInputRef}
                      type="file"
                      accept={formType === 'Hero' ? '.png' : '.ico,.png,.jpg'}
                      style={{ display: 'none' }}
                      onChange={handleIconChosen}
                    />
                    {formType !== 'Hero' && iconPreviewUrl && (
                      <div className="w-12 h-12 bg-surface-chip/50 border border-border rounded-xl p-1.5 shadow-sm overflow-hidden transition-all group-hover:scale-105">
                        <img src={iconPreviewUrl} alt="icon preview" className="w-full h-full object-cover rounded-lg" />
                      </div>
                    )}
                    {formType === 'Hero' && heroPreviewUrl && (
                      <div className="h-16 w-32 bg-surface-chip/50 border border-border rounded-xl p-1.5 shadow-sm overflow-hidden transition-all group-hover:scale-105">
                        <img src={heroPreviewUrl} alt="hero preview" className="h-full w-full object-cover rounded-lg" />
                      </div>
                    )}
                  </div>
                </div>

                <div className="w-full md:w-40 space-y-2.5">
                  <label className="block text-[10px] font-black text-text-secondary uppercase tracking-[0.15em] ml-1">Reminder (h)</label>
                  <input
                    type="number"
                    min="0"
                    placeholder="1"
                    value={formState.reminderHours}
                    onChange={(e) => update('reminderHours', e.target.value)}
                    className="input h-11"
                  />
                </div>
              </div>

              <div className="space-y-8">
                <div className="space-y-3">
                  <div className="space-y-2.5">
                    <label className="block text-[10px] font-black text-text-secondary uppercase tracking-[0.15em] ml-1">Interaction Link (URL/Path)</label>
                    <input
                      className="input"
                      placeholder="https:// | file:// | \UNC\Path"
                      value={formState.linkUrl}
                      onChange={(e) => update('linkUrl', e.target.value)}
                    />
                  </div>
                  <div className="flex items-start gap-2 text-text-tertiary px-1">
                    <Globe className="w-3 h-3 mt-0.5 opacity-50" />
                    <p className="text-[10px] font-medium leading-relaxed italic opacity-70">If no scheme is provided, the core will attempt to normalize system paths automatically.</p>
                  </div>
                </div>

                <div className="flex flex-col gap-4 p-4 bg-surface-chip/10 rounded-xl border border-border/50">
                  <label className="flex items-center gap-3 cursor-pointer group">
                    <input
                      type="checkbox"
                      className="w-5 h-5 rounded border-border bg-input text-primary-main focus:ring-primary-main"
                      checked={formState.soundEnabled}
                      onChange={(e) => update('soundEnabled', e.target.checked)}
                    />
                    <span className="text-sm font-bold text-text-secondary group-hover:text-text-primary transition-colors">Play Notification sound</span>
                  </label>
                </div>
              </div>
            </div>
          </div>
        )}
      </div>

      {scriptCardVisible && (
        <div className="w-full mt-12 pt-10 border-t border-border space-y-8">
          <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
            <div className="flex items-center gap-4">
              <div className="p-2.5 bg-pink-500/10 text-pink-600 dark:text-pink-400 rounded-xl border border-pink-500/20 shadow-sm">
                <Terminal className="w-5 h-5" />
              </div>
              <div className="flex flex-col">
                <h4 className="text-xl font-black text-text-primary tracking-tight uppercase">Logic Layer</h4>
                <p className="text-[10px] font-bold text-text-tertiary uppercase tracking-widest mt-0.5">Automated PowerShell Execution</p>
              </div>
            </div>
            <button
              className="btn btn-secondary px-6 text-xs h-11"
              type="button"
              onClick={onOpenTemplates}
            >
              PowerShell Templates
            </button>
          </div>

          <div className="card bg-background/40 p-0 border-border/70 overflow-hidden">
            <div className="p-6">
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
            </div>
            <div className="bg-surface-chip/30 px-6 py-3 border-t border-border/50">
              <p className="text-[10px] font-bold text-text-tertiary uppercase tracking-widest">Help: Execution requires PowerShell 5.1+ environment on endpoints.</p>
            </div>
          </div>
        </div>
      )}

      {/* Footer Actions */}
      <div className="flex justify-end gap-3 pt-6 border-t border-border">
        <button onClick={onNew} className="btn btn-secondary h-12 px-8">Reset Form</button>
        <button
          disabled={disableSave}
          onClick={onSave}
          className="btn btn-primary h-12 px-10"
        >
          Save Module
        </button>
      </div>
    </div>
  );
};
