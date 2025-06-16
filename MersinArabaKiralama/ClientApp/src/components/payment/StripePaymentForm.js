import React, { useState } from 'react';
import {
  CardElement,
  useStripe,
  useElements,
} from '@stripe/react-stripe-js';
import { Button, Typography, Box, Alert } from '@mui/material';
import { LoadingButton } from '@mui/lab';

const CARD_ELEMENT_OPTIONS = {
  style: {
    base: {
      color: '#32325d',
      fontFamily: '"Helvetica Neue", Helvetica, sans-serif',
      fontSmoothing: 'antialiased',
      fontSize: '16px',
      '::placeholder': {
        color: '#aab7c4',
      },
    },
    invalid: {
      color: '#fa755a',
      iconColor: '#fa755a',
    },
  },
};

const StripePaymentForm = ({ onSuccess, onError }) => {
  const stripe = useStripe();
  const elements = useElements();
  const [error, setError] = useState(null);
  const [processing, setProcessing] = useState(false);
  const [disabled, setDisabled] = useState(true);
  const [succeeded, setSucceeded] = useState(false);
  const [clientSecret, setClientSecret] = useState('');

  const handleChange = async (event) => {
    setDisabled(event.empty);
    setError(event.error ? event.error.message : '');
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    
    if (!stripe || !elements) {
      return;
    }

    setProcessing(true);

    try {
      // Burada backend'den payment intent oluşturulmalı
      // const { clientSecret } = await createPaymentIntent(amount);
      // setClientSecret(clientSecret);
      
      // Ödeme işlemini onayla
      const payload = await stripe.confirmCardPayment(clientSecret, {
        payment_method: {
          card: elements.getElement(CardElement),
          billing_details: {
            // Müşteri bilgileri buraya gelebilir
          },
        },
      });

      if (payload.error) {
        setError(`Ödeme işlemi başarısız: ${payload.error.message}`);
        if (onError) onError(new Error(payload.error.message));
      } else {
        setError(null);
        setSucceeded(true);
        if (onSuccess) onSuccess(payload.paymentIntent);
      }
    } catch (err) {
      const errorMessage = err.message || 'Ödeme işlemi sırasında bir hata oluştu';
      setError(errorMessage);
      if (onError) onError(new Error(errorMessage));
    } finally {
      setProcessing(false);
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      <Box sx={{ mb: 3 }}>
        <CardElement 
          options={CARD_ELEMENT_OPTIONS} 
          onChange={handleChange}
        />
      </Box>
      
      {error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {error}
        </Alert>
      )}
      
      <LoadingButton
        type="submit"
        fullWidth
        variant="contained"
        color="primary"
        size="large"
        disabled={!stripe || disabled || processing || succeeded}
        loading={processing}
      >
        {succeeded ? 'Ödeme Alındı' : 'Ödemeyi Onayla'}
      </LoadingButton>
      
      <Typography variant="caption" display="block" mt={2} color="text.secondary" align="center">
        Ödeme işleminiz güvenli bir şekilde gerçekleştirilecektir.
      </Typography>
    </form>
  );
};

export default StripePaymentForm;
