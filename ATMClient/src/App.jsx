import { useState, useEffect } from 'react'
import { v4 as uuid } from 'uuid'
import './App.css'

const API_BASE = "http://localhost:5056";

export default function App() {
  const [userForm, setUserForm] = useState({
    email: "",
    firstName: "",
    lastName: "",
  });

  const [error, setError] = useState(null);
  const [user, setUser] = useState(null);
  const [accounts, setAccounts] = useState([]);
  const [selectedAccountId, setSelectedAccountId] = useState();
  const [selectedAccount, setSelectedAccount] = useState(null);
  const [transactions, setTransactions] = useState([]);

  const [amount, setAmount] = useState("");
  const [transferAccountId, setTransferAccountId] = useState("");

  const moneyRegex = /^\d+(\.\d{0,2})?$/;
  const isAmountValid = amount !== "" && moneyRegex.test(amount)
    && parseFloat(amount) > 0;

  const isTransferValid = isAmountValid && transferAccountId 
    && transferAccountId !== selectedAccountId;

  function showAPIError(errorText, defaultMessage) {
    let errorAsJson;
    let errorMessage;
    if (errorText) {
      errorAsJson = JSON.parse(errorText);
      errorMessage = `${errorAsJson.status}: ${errorAsJson.title} ${Object.values(errorAsJson.errors)}`;
    }
    setError(errorMessage || "Some error occured with user creation");
    setTimeout(() => setError(""), 3000);
  }

  async function createUser(e) {
    e.preventDefault();

    const res = await fetch(`${API_BASE}/api/users`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(userForm),
    });

    if (!res.ok) {
      const errorText = await res.text();
      showAPIError(errorText, "Some error occured with user creation");
      return;
    }

    const data = await res.json();
    setUser({
      id: data.id,
      email: data.email,
      firstName: data.firstName,
      lastName: data.lastName
    });
    setAccounts(data.accounts);
    setSelectedAccountId(data.accounts[0].id)
    setSelectedAccount(data.accounts[0]);
  }

  async function loadTransactions() {
    if (!selectedAccountId) return;

    const res = await fetch(
      `${API_BASE}/api/transactions?accountId=${selectedAccountId}`
    );
    const data = await res.json();
    setTransactions(data);
  }

  async function loadAccount() {
    if (!selectedAccountId) return;

    const res = await fetch(
      `${API_BASE}/api/accounts?id=${selectedAccountId}`
    );

    const data = await res.json();
    setSelectedAccount(data)
    setTransactions(data.transactions);
  }

  useEffect(() => {
    if (selectedAccountId)
      loadAccount();
  }, [selectedAccountId])

  async function createTransaction(action, from, to) {
    if (!selectedAccountId || !amount) return;

    let url = "";
    let body = {
      id: uuid(),
      amount: parseFloat(amount),
      fromAccountId: from,
      toAccountId: to,
    };

    switch (action){
      case "deposit":
        body.type = 0;
        break;
      case "withdraw":
        body.type = 1;
        break;
      default:
        body.type = 2;
    }

    const res = await fetch(
      `${API_BASE}/api/transactions`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    });

    if (!res.ok) {
      const errorText = await res.text();
      showAPIError(errorText, "Some error occured with transaction");
      return;
    }

    setAmount("");
    loadAccount();
  }

  function transactionTypeToText(type) {
    switch (type) {
      case 0: return "💰Deposit";
      case 1: return "💸Withdraw";
      case 2: return "💱Transfer";
      default: return type;
    }
  }

  function transactionStatusToText(status) {
    switch (status) {
      case 0: return "Unknown";
      case 3: return "Processed";
      case 4: return "Invalid";
      case 5: return "Failed 💬";
      default: return status;
    }
  }

  function formatDate(date) {
    if (!date || date < '2025-01-01') return "-";
    try {
      return new Date(date).toLocaleString();
    } catch {
      return date;
    }
  }

  const th = {
    borderBottom: "2px solid #ddd",
    padding: "10px",
    textAlign: "left",
    fontWeight: "bold",
    background: "#f0f0f0",
  };

  const td = {
    borderBottom: "1px solid #eee",
    padding: "8px",
  };

  return (
    <div style={{ fontFamily: "sans-serif", padding: 20 }}>
      <h1>ATM Project</h1>

      {/* user creation form */}
      {!user && (
        <section style={{ marginBottom: 30 }}>
          <h2>Create User</h2>
          <form onSubmit={createUser} style={{ display: "flex", gap: 10 }}>
            <input
              type="email"
              required
              placeholder="Email"
              value={userForm.email}
              onChange={(e) =>
                setUserForm({ ...userForm, email: e.target.value })
              }
            />
            <input
              type="text"
              required
              placeholder="First Name"
              value={userForm.firstName}
              onChange={(e) =>
                setUserForm({ ...userForm, firstName: e.target.value })
              }
            />
            <input
              type="text"
              required
              placeholder="Last Name"
              value={userForm.lastName}
              onChange={(e) =>
                setUserForm({ ...userForm, lastName: e.target.value })
              }
            />
            <button type="submit">Create</button>
          </form>
        </section>
      )}

      {/* welcome message */}
      {user && (
        <section>
          <h3>Welcome {user.firstName} {user.lastName}</h3>
        </section>
      )}

      {/* select account */}
      {accounts.length > 0 && (
        <section style={{ marginTop: 20 }}>
          <h2>Accounts</h2>

          <select
            value={selectedAccountId}
            onChange={(e) => setSelectedAccountId(e.target.value)}
            style={{ padding: 5 }}
          >
            {accounts.map((a) => (
              <option key={a.id} value={a.id}>
                {a.id} - {a.type === 0 ? "Savings" : "Checking"}
              </option>
            ))}
          </select>

          <h3>Balance ${selectedAccount?.balance?.toFixed(2)}</h3>
        </section>
      )}

      {/* perform transactions */}
      {selectedAccountId && (
        <section style={{ marginTop: 30 }}>
          <h2>Actions</h2>

          <div style={{ display: "flex", gap: 10 }}>
            <input
              type="number"
              placeholder="Amount"
              value={amount}
              onChange={(e) => {
                const value = e.target.value;

                if (value === "") {
                  setAmount("");
                  return;
                }

                if (moneyRegex.test(value)) {
                  setAmount(value);
                }
              }}
              style={{
                border: (amount !== "" && !isAmountValid) ? "2px solid red" : "1px solid #ccc",
                padding: 5,
              }}
            />

            <button
              disabled={!isAmountValid || !selectedAccountId}
              onClick={() => createTransaction("deposit", selectedAccountId)}>
                Deposit
            </button>
            <button
              disabled={!isAmountValid || !selectedAccountId}
              onClick={() => createTransaction("withdraw", selectedAccountId)}>
                Withdraw
            </button>

            {/* transfer */}
            <select
              value={transferAccountId}
              onChange={(e) => setTransferAccountId(e.target.value)}
            >
              <option value="">Transfer To...</option>
              {accounts
                .filter((a) => a.id !== selectedAccount?.id)
                .map((a) => (
                  <option key={a.id} value={a.id}>
                    {a.id}
                  </option>
                ))}
            </select>

            <button 
              disabled={!isTransferValid}
              onClick={() => createTransaction("transfer", selectedAccountId, transferAccountId)}>
              Transfer
            </button>
          </div>

          <h3 style={{ marginTop: 20 }}>Transactions</h3>

          <table
            style={{
              width: "100%",
              borderCollapse: "collapse",
              marginTop: "20px",
              background: "#fafafa",
            }}
          >
            <thead>
              <tr>
                <th style={th}>ID</th>
                <th style={th}>Amount</th>
                <th style={th}>From</th>
                <th style={th}>To</th>
                <th style={th}>Type</th>
                <th style={th}>Status</th>
                <th style={th}>Created</th>
                <th style={th}>Processed</th>
              </tr>
            </thead>

            <tbody>
              {transactions.length === 0 && (
                <tr>
                  <td colSpan="9" style={{ textAlign: "center", padding: 20 }}>
                    No transactions found
                  </td>
                </tr>
              )}

              {transactions.map((t) => (
                <tr key={t.id}>
                  <td style={td}>{t.id}</td>
                  <td style={td}>${t.amount.toFixed(2)}</td>
                  <td style={td}>{t.fromAccountId ?? "-"}</td>
                  <td style={td}>{t.toAccountId === 0 ? "-" : t.toAccountId}</td>
                  <td style={td}>{transactionTypeToText(t.type)}</td>
                  <td style={td} title={t.message}>{transactionStatusToText(t.status)}</td>
                  <td style={td}>{formatDate(t.createdAt)}</td>
                  <td style={td}>{formatDate(t.processedAt)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </section>
      )}

      {/* error banner */}
      {error && (
        <div
          style={{
            position: "fixed",
            top: 20,
            right: 20,
            background: "red",
            color: "white",
            padding: "10px 15px",
            borderRadius: 8,
          }}
        >
          {error}
        </div>
      )}
    </div>
  );
}
