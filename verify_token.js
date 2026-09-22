const crypto = require('crypto');
const token = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImZiOTk1ZjdlLWYwYWEtNGI4Ni1iMWYwLTdjOGUxYjQwY2M5OCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImFkYXlAdGVzdC5jb20iLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQXnFn2UgQWRheSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkNhbmRpZGF0ZSIsImV4cCI6MTc5MDA2Mjk5NywiaXNzIjoiS2FyaXllck5ldEFwaSIsImF1ZCI6Ikthcml5ZXJOZXRVc2VycyJ9.ScunolrXZbUPfkEkIooeoh1cX9bGI3xBPT0D3cw5R2';
const key = '3f8a1c9e2b4d6f7a9c1e3b5d7f9a1c3e5b7d9f1a3c5e7b9d1f3a5c7e9b1d3f5a';
const parts = token.split('.');
const sigInput = parts[0] + '.' + parts[1];
const sig = crypto.createHmac('sha256', key).update(sigInput).digest('base64url');
console.log('Expected sig:', sig);
console.log('Token sig:   ', parts[2]);
console.log('Match:', sig === parts[2]);
