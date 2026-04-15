<script setup lang="ts">



</script> 


<template>
  <div class="mainContainer">
    <div>
        <h1 class="text-2xl font-bold mb-4">404 - Sidan hittades inte</h1>
        <br>
        <p class="mb-4">Matchen du letade fanns inte.</p>
        <br>
        <p>Antingen är matchId felaktid eller så har matchen tagits bort.</p>
        <br>
        <h3>Sry team jag ska få mer gjort nästa gång jag sätter mig. puss puss</h3>
    </div>
    <div>

        <router-link to="/" class="backBtn">Tillbaka till startsidan</router-link>
    </div>
  </div>
  </template>

  <style scoped>
  .mainContainer {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: space-between;
    max-width: 800px;
    height: 70vh;
    margin: 50px auto;
    padding: 20px;
    background-color: #f9f9f9;
    border-radius: 8px;
    text-align: center;
  }
  .backBtn {
    display: inline-block;
    width: 200px;
    margin-top: 20px;
    padding: 10px 20px;
    background-color: #007bff;
    color: white;
    text-decoration: none;
    border-radius: 4px;
  }
  .backBtn:hover {
    background-color: #0056b3;
  }
  
  
  </style>