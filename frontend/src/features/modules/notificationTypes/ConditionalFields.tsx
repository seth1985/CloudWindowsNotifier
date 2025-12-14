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
  // These are accepted to satisfy the parent signature but handled in the General section.
  title: _title,
  setTitle: _setTitle,
  message: _message,
  setMessage: _setMessage,
  conditionalInterval,
  setConditionalInterval,
  conditionalScript,
  setConditionalScript
}) => (
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
          value={conditionalInterval}
          onChange={(e) => setConditionalInterval(e.target.value)}
          className="narrow-input"
        />
      </label>
    </div>
    <label>
      Conditional script (PowerShell)
      <textarea
        placeholder="Write or paste the conditional script..."
        value={conditionalScript}
        onChange={(e) => setConditionalScript(e.target.value)}
        className="script-area"
      />
    </label>
  </div>
);
