'use client';

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import AddCreditsModal from '../../components/AddCreditsModal';

export default function PlayerClient({ _id }) {
  const router = useRouter();
  const [playerName, setPlayerName] = useState('');
  const [playerId, setPlayerId] = useState('');
  const [playerPfp, setAvatarUrl] = useState('');
  const [balance, setBalance] = useState(0);
  const [showModal, setShowModal] = useState(false);
  const [creditsToAdd, setCreditsToAdd] = useState('');
  // const [apiBaseUrl] = process.env.NEXT_PUBLIC_API_URL || 'https://localhost:7069';

  // Client-side auth guard
  useEffect(() => {
    const apiBaseUrl = process.env.NEXT_PUBLIC_API_URL || 'https://localhost:7069';
    fetch(`${apiBaseUrl}/auth/me`, { credentials: 'include' })
      .then((res) => {
        if (!res.ok) {
          router.replace('/login');
        } else {
          res.json().then((data) => {
            // console.log('Authenticated user:', data);
            // setPlayerName(data.claims[2].value);
          });
        }
      })
      .catch((err) => {
        // console.error('Auth check failed:', err);
        router.replace('/login');
      });

    // get user {id} from auth connection
    // console.log('Fetching user data');
    fetch(`${apiBaseUrl}/api/user/me`, { credentials: 'include' })
      .then((res) => {
        // console.log('User data response received');
        if (!res.ok) {
          // console.log('Failed to fetch user data');
          router.replace('/rooms');
        } else {
          res.json().then((data) => {
            // console.log('User found:', data);
            setPlayerId(data.id);
            setPlayerName(data.name);
            setBalance(data.balance);
            setAvatarUrl(data.avatarUrl);
          });
        }
      });
  }, [router]);

  const handleAddCredits = async (e) => {
    e.preventDefault();
    const apiBaseUrl = process.env.NEXT_PUBLIC_API_URL || 'https://localhost:7069';

    const amount = parseFloat(creditsToAdd);
    if (Number.isNaN(amount) || amount <= 0) return;

    // compute first (state updates are async)
    const newBalance = balance + amount;

    try {
      // console.log('Patch request sending');
      const res = await fetch(`${apiBaseUrl}/api/user/${playerId}`, {
        method: 'PATCH',
        credentials: 'include', // <-- send the auth cookie
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ balance: newBalance }), // <-- matches UpdateBalanceRequest
      });

      // console.log('Balance update response received');

      if (!res.ok) {
        // console.log('Failed to update balance', res.status, await res.text());
        if (res.status === 401) {
          // lost auth — kick to login
          router.replace('/login');
        }
        return;
      }

      // success: update UI
      setBalance(newBalance);
      setCreditsToAdd('');
      setShowModal(false);
      // console.log('Balance updated successfully');
    } catch (err) {
      // console.error('Network error on PATCH', err);
    }
  };

  const handleCloseModal = () => {
    setShowModal(false);
    setCreditsToAdd('');
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-green-900 via-green-800 to-emerald-900 p-8 relative overflow-hidden flex items-center justify-center">
      <div className="flex flex-col items-center justify-center w-full max-w-md mx-auto">
        {/* Player Avatar */}
        <img
          src={playerPfp}
          alt="Gamer Avatar"
          className="w-32 h-32 mb-4 rounded-full overflow-hidden border-4 border-yellow-600 shadow-lg"
        />
        {/* Player Name */}
        <h1 className="text-4xl font-bold bg-gradient-to-b from-yellow-400 via-yellow-500 to-yellow-600 bg-clip-text text-transparent mb-4 text-center">
          {playerName}
        </h1>
        {/* Player Credits */}
        <div className="bg-black/50 rounded-lg p-4 mb-6 border border-yellow-600 max-w-xs w-full text-center">
          <p className="text-gray-300 text-sm mb-1">Current Credits</p>
          <p className="text-3xl font-bold text-yellow-400">{balance} Devito Bucks</p>
        </div>
        {/* Add Credits Button */}
        <button
          onClick={() => setShowModal(true)}
          className="px-6 py-3 bg-gradient-to-r from-yellow-400 via-yellow-500 to-yellow-600 text-black font-bold rounded-lg hover:from-yellow-500 hover:to-yellow-700 transition-all duration-200 shadow-[0_0_15px_rgba(234,179,8,0.5)] hover:shadow-[0_0_25px_rgba(234,179,8,0.8)] border-2 border-yellow-700"
          type="button"
        >
          Add Credits
        </button>
      </div>

      <AddCreditsModal
        isOpen={showModal}
        onClose={handleCloseModal}
        balance={balance}
        creditsToAdd={creditsToAdd}
        setCreditsToAdd={setCreditsToAdd}
        onSubmit={handleAddCredits}
      />
    </div>
  );
}
