import React from 'react';

type Props = {
  title?: string;
  setTitle?: (v: string) => void;
  dynamicScript: string;
  setDynamicScript: (v: string) => void;
  dynamicMaxLength: string;
  setDynamicMaxLength: (v: string) => void;
  dynamicTrimWhitespace: boolean;
  setDynamicTrimWhitespace: (v: boolean) => void;
  dynamicFailIfEmpty: boolean;
  setDynamicFailIfEmpty: (v: boolean) => void;
  dynamicFallbackMessage: string;
  setDynamicFallbackMessage: (v: string) => void;
};

export const DynamicFields: React.FC<Props> = ({
  title: _title,
  setTitle: _setTitle,
  dynamicScript,
  setDynamicScript,
  dynamicMaxLength,
  setDynamicMaxLength,
  dynamicTrimWhitespace,
  setDynamicTrimWhitespace,
  dynamicFailIfEmpty,
  setDynamicFailIfEmpty,
  dynamicFallbackMessage,
  setDynamicFallbackMessage
}) => (
  <div className="space-y-8">
    <div className="flex flex-col gap-2 p-5 bg-primary-main/5 border border-primary-main/20 rounded-xl">
      <h4 className="text-[10px] font-black text-primary-main uppercase tracking-[0.2em]">Execution Policy: Dynamic Content</h4>
      <p className="text-xs font-bold text-text-secondary">Runs a PowerShell script that returns title/message/link/icon metadata dynamically.</p>
    </div>

    <div className="space-y-2.5">
      <label className="block text-[10px] font-black text-text-secondary uppercase tracking-[0.15em] ml-1">Script Template (PowerShell)</label>
      <textarea
        placeholder="# Example: Write-Output 'Dynamic Message'"
        rows={8}
        value={dynamicScript}
        onChange={(e) => setDynamicScript(e.target.value)}
        className="w-full bg-black/40 border border-border-input rounded-xl px-4 py-4 text-[11px] text-green-400 font-mono focus:ring-2 focus:ring-primary-main/50 outline-none transition-all resize-none leading-relaxed"
      />
    </div>

    <div className="grid grid-cols-1 md:grid-cols-2 gap-10 items-start">
      <div className="space-y-2.5">
        <label className="block text-[10px] font-black text-text-secondary uppercase tracking-[0.15em] ml-1">Maximum Content Length</label>
        <input
          type="number"
          min="1"
          value={dynamicMaxLength}
          onChange={(e) => setDynamicMaxLength(e.target.value)}
          className="input"
        />
      </div>

      <div className="space-y-4 pt-4">
        <label className="flex items-center gap-4 cursor-pointer group">
          <input
            type="checkbox"
            className="w-5 h-5 rounded border-border bg-input text-primary-main focus:ring-primary-main transition-all cursor-pointer"
            checked={dynamicTrimWhitespace}
            onChange={(e) => setDynamicTrimWhitespace(e.target.checked)}
          />
          <span className="text-sm font-bold text-text-secondary group-hover:text-text-primary transition-colors">Trim whitespace</span>
        </label>
        <label className="flex items-center gap-4 cursor-pointer group">
          <input
            type="checkbox"
            className="w-5 h-5 rounded border-border bg-input text-primary-main focus:ring-primary-main transition-all cursor-pointer"
            checked={dynamicFailIfEmpty}
            onChange={(e) => setDynamicFailIfEmpty(e.target.checked)}
          />
          <span className="text-sm font-bold text-text-secondary group-hover:text-text-primary transition-colors">Fail on empty result</span>
        </label>
      </div>

      <div className="space-y-2.5 md:col-span-2">
        <label className="block text-[10px] font-black text-text-secondary uppercase tracking-[0.15em] ml-1">Execution Fallback Message</label>
        <input
          className="input"
          placeholder="Message to display if logic fails or returns null..."
          value={dynamicFallbackMessage}
          onChange={(e) => setDynamicFallbackMessage(e.target.value)}
        />
      </div>
    </div>
  </div>
);
