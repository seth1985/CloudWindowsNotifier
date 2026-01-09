import React from 'react';

type Props = {
  title?: string;
  setTitle?: (v: string) => void;
  message?: string;
  setMessage?: (v: string) => void;
  conditionalInterval: string;
  setConditionalInterval: (v: string) => void;
  conditionalScript: string;
  setConditionalScript: (v: string) => void;
};

export const ConditionalFields: React.FC<Props> = ({
  title: _title,
  setTitle: _setTitle,
  message: _message,
  setMessage: _setMessage,
  conditionalInterval,
  setConditionalInterval,
  conditionalScript,
  setConditionalScript
}) => (
  <div className="space-y-8">
    <div className="flex flex-col gap-2 p-5 bg-primary-main/5 border border-primary-main/20 rounded-xl">
      <h4 className="text-[10px] font-black text-primary-main uppercase tracking-[0.2em]">Execution Policy: Conditional</h4>
      <p className="text-xs font-bold text-text-secondary">Runs a PowerShell script; toast will only trigger if script exits with Code 0.</p>
    </div>

    <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
      <div className="space-y-2.5">
        <label className="block text-[10px] font-black text-text-secondary uppercase tracking-[0.15em] ml-1">Polling Frequency (min)</label>
        <input
          type="number"
          min="1"
          value={conditionalInterval}
          onChange={(e) => setConditionalInterval(e.target.value)}
          className="input"
        />
      </div>
    </div>

    <div className="space-y-2.5">
      <label className="block text-[10px] font-black text-text-secondary uppercase tracking-[0.15em] ml-1">PowerShell Verification Script</label>
      <textarea
        placeholder="# Example: Exit (Test-Path 'C:\Temp')"
        rows={8}
        value={conditionalScript}
        onChange={(e) => setConditionalScript(e.target.value)}
        className="w-full bg-black/40 border border-border-input rounded-xl px-4 py-4 text-[11px] text-green-400 font-mono focus:ring-2 focus:ring-primary-main/50 outline-none transition-all resize-none leading-relaxed"
      />
    </div>
  </div>
);
